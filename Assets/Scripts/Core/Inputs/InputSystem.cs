using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.DI;
using Core.Exceptions;
using Core.GlobalEvent;
using Core.GlobalEvent.Events;
using Core.Inputs.Providers;
using Core.Log;
using Core.Mono;
using Core.Pool;
using Core.Serialize.Json;
using Core.Utility;
using UnityEngine.InputSystem;
using Logger = Core.Log.Logger;

namespace Core.Inputs
{
    /// <summary>
    /// 输入系统核心管理类
    /// 负责输入动作的初始化、启用/禁用、按键修改、冲突检测等核心逻辑
    /// 继承单例基类，保证全局唯一实例；实现IInputSystem接口（接口未展示）
    /// </summary>
    public class InputSystem : IInputSystem, IApplicationExitNotify
    {
        public int QuitPriority => 1;
        
        [Inject] private IJsonManager jsonManager;
        [Inject] private IEventCenter eventCenter;
        [Inject] private IPoolManager poolManager;
        
        // 输入配置的JSON原始数据
        private string _defaultJsonInputData;
        // 改键后的json覆盖json数据
        private string _overrideJsonInputData;
        // 玩家输入组件引用，关联InputActionAsset
        private PlayerInput _playerInput;
        // 
        private InputActionAsset _inputActionAsset;
        
        private InputSystem(IMonoAdapter monoAdapter)
        {
            monoAdapter.AddApplicationExitNotify(this);
        }

        /// <summary>
        /// 初始化输入系统
        /// </summary>
        /// <param name="provider"></param>
        public async Task InitSystem(IInputDataProvider provider)
        {
            await provider.LoadDataAsync();
            if (provider.TryGetData(out var defaultJson, out var overrideJson))
            {
                _defaultJsonInputData = defaultJson;
                _inputActionAsset = InputActionAsset.FromJson(_defaultJsonInputData);
                if(string.IsNullOrEmpty(overrideJson))
                {
                    var messageEvent = EventSource.Get<GlobalMessageEvent>();
                    messageEvent.Message = "本地输入数据加载失败";
                    Logger.LogError(ELogTags.Input, $"OverrideJson is empty");
                    eventCenter.TriggerEventAsync(messageEvent);
                }
                else
                {
                    _overrideJsonInputData = overrideJson;
                    _inputActionAsset.LoadBindingOverridesFromJson(_overrideJsonInputData);
                }
            }
            else
            {
                throw ExceptionHelper.Throw("Default json is empty");
            }
        }

        /// <summary>
        /// 初始化玩家输入组件
        /// </summary>
        /// <param name="playerInput">玩家输入组件实例</param>
        /// <param name="onActionTrigger">输入动作触发时的回调方法</param>
        /// <returns>异步任务</returns>
        public void InitPlayerInput(PlayerInput playerInput, Action<InputAction.CallbackContext> onActionTrigger)
        {
            if (!playerInput || onActionTrigger == null)
                throw ExceptionHelper.Throw("playerInput or onActionTrigger is null");
            
            // 缓存玩家输入组件引用
            _playerInput = playerInput;
            // 设置通知行为为调用C#事件
            _playerInput.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;
            // 注册动作触发回调
            _playerInput.onActionTriggered += onActionTrigger;
            UpdateActions();
        }

        /// <summary>
        /// 重置玩家输入
        /// </summary>
        public void ResetPlayerInput()
        {
            _playerInput = null;
        }

        /// <summary>
        /// 启用所有输入动作
        /// 使输入系统响应玩家输入
        /// </summary>
        public void Enable()
        {
            _playerInput.actions?.Enable();
        }

        /// <summary>
        /// 禁用所有输入动作
        /// 使输入系统停止响应玩家输入
        /// </summary>
        public void Disable()
        {
            _playerInput.actions?.Disable();
        }

        public void SwitchScheme(string scheme)
        {
            _playerInput.SwitchCurrentControlScheme(scheme);
        }

        /// <summary>
        /// 根据动作名称获取输入动作实例
        /// </summary>
        /// <param name="actionName">输入动作名称</param>
        /// <returns>对应的InputAction实例</returns>
        private InputAction FindInputAction(string actionName)
        {
            return _playerInput.actions.FindAction(actionName);
        }

        public BindingInfo GetCurrentBinding(string actionName, int bindingIndex)
        {
            var action = FindInputAction(actionName);
            return new BindingInfo(action.bindings[bindingIndex]);
        }

        public IEnumerable<BindingInfo> GetCurrentBindings(string actionName)
        {
            foreach (var inputBinding in FindInputAction(actionName).bindings)
            {
                if (inputBinding.isComposite)
                    continue;
                
                yield return new BindingInfo(inputBinding);
            }
        }
        
        public IEnumerable<string> GetActiveActions()
        {
            foreach (var inputAction in _playerInput.actions)
            {
                yield return inputAction.name;
            }
        }
        
        public ReeditOperation EditInput(string actionName, string bindingName)
        {
            // 判断Action的输入类型，复合Action，先找到单个按键绑定的索引
            return EditInputInternal(actionName, action => action.type != InputActionType.Button ? action.bindings.IndexOf(bd => bd.name == bindingName) : 0);
        }

        public ReeditOperation EditInput(string actionName, int bindIndex)
        {
            // 使用外部指定的绑定索引
            return EditInputInternal(actionName, _ => bindIndex);
        }

        private ReeditOperation EditInputInternal(string actionName, Func<InputAction, int> getBingingIndex)
        {
            // 先禁用输入
            Disable();
            // 找到要修改的
            var action = FindInputAction(actionName);
            if (action == null)
                throw ExceptionHelper.Throw($"Current inputAction not found for {actionName}");
            
            var bindingIndex = getBingingIndex(action);
            // 获取取消键
            var cancelKey = GetCancelPathForCurrentScheme();
            // 复用编辑对象
            var editOperation = poolManager.GetData<ReeditOperation>();
            // 开始监听修改按键
            action.PerformInteractiveRebinding(bindingIndex)
                .WithCancelingThrough(cancelKey)
                .OnApplyBinding((operation, bindingPath) =>
                {
                    if (HasKeyConflict(actionName, bindingIndex, bindingPath, out var conflictInfo))
                    {
                        editOperation.ConflictType = EKeyConflict.ExistKey;
                        editOperation.onConflict += isExchange =>
                        {
                            if (isExchange)
                            {
                                ExecuteExchangeKey(conflictInfo);
                            }

                            Enable();
                            operation.Dispose();
                        };

                        // 间接调用外部回调
                        editOperation.Apply();
                        return;
                    }
                    
                    var newBinding = new InputBinding
                    {
                        overridePath = bindingPath,
                        overrideInteractions = action.bindings[bindingIndex].effectiveInteractions
                    };
                    operation.action.ApplyBindingOverride(bindingIndex, newBinding);
                    // 保存覆盖数据
                    _overrideJsonInputData = _playerInput.actions.SaveBindingOverridesAsJson();
                    editOperation.ConflictType = EKeyConflict.Over;
                    // 间接调用外部回调
                    editOperation.Apply();
                    Enable();
                    operation.Dispose();
                })
                .OnCancel(operation =>
                {
                    // 用户取消
                    editOperation.Cancel();
                    Enable();
                    operation.Dispose();
                }).Start();
            
            return editOperation;
        }
        
        /// <summary>
        /// 获取当前的改建取消路径
        /// </summary>
        /// <returns></returns>
        private string GetCancelPathForCurrentScheme()
        {
            // 如果没有设置 PlayerInput 或没有活跃方案，就返回键鼠默认
            if (!_playerInput || _playerInput.currentControlScheme == null)
                return "<Keyboard>/escape";

            return _playerInput.currentControlScheme switch
            {
                "Keyboard&Mouse" => "<Keyboard>/escape",
                "Gamepad" => "<Gamepad>/buttonEast",
                _ => "<Keyboard>/escape"
            };
        }

        /// <summary>
        /// 更新输入动作配置
        /// 将最新的InputActionAsset赋值给PlayerInput，使修改生效
        /// </summary>
        /// <param name="playerInput">可选：指定新的PlayerInput实例；否则用<see cref="InitPlayerInput"/>中传入的实例</param>
        public void UpdateActions(PlayerInput playerInput = null)
        {
            if (playerInput)
            {
                // 赋值新的InputActionAsset并更新引用
                playerInput.actions = _inputActionAsset;
                _playerInput = playerInput;
            }
            else if (_playerInput)
            {
                // 刷新现有PlayerInput的动作配置
                _playerInput.actions = _inputActionAsset;
                Logger.LogDebug(ELogTags.Input, $"Input configuration update successful,{_playerInput.actions}");
            }
            else
            {
                // 日志：PlayerInput为空，更新失败
                Logger.LogError(ELogTags.Input, $"Input configuration acquisition failed,{playerInput}");
            }

            if (_playerInput)
            {
#if UNITY_STANDALONE_WIN
                SwitchScheme("PC");
#elif UNITY_ANDROID
                SwitchScheme("Android");
#endif
            }
        }

        /// <summary>
        /// 检查按键是否冲突
        /// 1. 同一动作映射下的相同路径不判定为冲突
        /// 2. 新按键已被其他动作映射使用则判定为冲突
        /// </summary>
        /// <param name="actionName">输入动作名称</param>
        /// <param name="bindIndex">输入动作对应的按键的绑定索引，当前重绑定的action的绑定索引</param>
        /// <param name="newPath"></param>
        /// <param name="conflictInfo"></param>
        /// <returns>true=冲突，false=无冲突</returns>
        private bool HasKeyConflict(string actionName, int bindIndex, string newPath, out ConflictInfo conflictInfo)
        {
            var actions = _playerInput.actions;
            var action = actions[actionName];
            // 获取当前正在改的绑定的交互方式（尚未被覆盖，所以是旧的 interactions）
            var currentInteractions = action.bindings[bindIndex].effectiveInteractions;
            foreach (var inputAction in actions)
            {
                for (var i = 0; i < inputAction.bindings.Count; i++)
                {
                    // 其它Action的绑定
                    var otherBinding = inputAction.bindings[i];
                    
                    if(otherBinding.isComposite)
                        continue;
                    
                    var samePath = otherBinding.effectivePath == newPath;
                    var sameInteraction = otherBinding.effectiveInteractions == currentInteractions;
                    var nonSameIndex = i != bindIndex;
                    var sameActon = otherBinding.action == actionName;
                    
                    // 自己改自己，不算冲突
                    if(!nonSameIndex && sameActon && samePath && sameInteraction)
                        continue;
                    
                    // 新路径与其它按键绑定路径相同，且触发方式也相同，且绑定索引不同，则存在冲突
                    if (samePath && sameInteraction)
                    {
                        conflictInfo = new ConflictInfo
                        {
                            CurrentBinding = action.bindings[bindIndex],
                            ConflictBinding = otherBinding,
                            CurrentBindingIndex = bindIndex,
                            ConflictBindingIndex = i
                        };
                        
                        return true;
                    }
                }
            }

            conflictInfo = default;
            return false;
        }
        
        private void ExecuteExchangeKey(ConflictInfo conflictInfo)
        {
            var actions = _playerInput.actions;
            // 当前重绑定Action
            var currentActon = actions[conflictInfo.CurrentBinding.action];
            // 将当前重绑定Action的绑定更改为冲突的绑定info
            InputBinding conflictBinding = new()
            {
                overridePath = conflictInfo.ConflictBinding.effectivePath,
                overrideInteractions = conflictInfo.ConflictBinding.effectiveInteractions
            };
            
            currentActon.RemoveBindingOverride(conflictInfo.CurrentBindingIndex);
            currentActon.ApplyBindingOverride(conflictInfo.CurrentBindingIndex, conflictBinding);
            // 冲突的绑定的Action
            var overrideActon = actions[conflictInfo.ConflictBinding.action];
            // 将冲突的绑定的Action的绑定更改为当前重绑定的info，即交换
            InputBinding currentBinding = new()
            {
                overridePath = conflictInfo.CurrentBinding.effectivePath,
                overrideInteractions = conflictInfo.CurrentBinding.effectiveInteractions
            };
            
            overrideActon.RemoveBindingOverride(conflictInfo.ConflictBindingIndex);
            overrideActon.ApplyBindingOverride(conflictInfo.ConflictBindingIndex, currentBinding);
            // 保存覆盖数据
            _overrideJsonInputData = _playerInput.actions.SaveBindingOverridesAsJson();
        }
        
        public void OnAppQuit()
        {
            jsonManager.SaveAsync(_overrideJsonInputData, PathUtility.GetUserDataLocalSavePath(FileSources.InputActionLocalFileName));
        }
    }
}