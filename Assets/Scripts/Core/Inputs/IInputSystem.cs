using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Inputs.Providers;
using UnityEngine.InputSystem;

namespace Core.Inputs
{
    /// <summary>
    /// 输入系统接口
    /// </summary>
    public interface IInputSystem
    {
        /// <summary>
        /// 初始化输入系统
        /// </summary>
        /// <param name="provider"></param>
        Task InitSystem(IInputDataProvider provider);
        
        /// <summary>
        /// 初始化玩家输入组件
        /// </summary>
        /// <param name="playerInput">玩家输入组件实例</param>
        /// <param name="onActionTrigger">输入动作触发时的回调方法</param>
        /// <returns>异步任务</returns>
        void InitPlayerInput(PlayerInput playerInput, Action<InputAction.CallbackContext> onActionTrigger);

        /// <summary>
        /// 启用输入
        /// </summary>
        void Enable();
        
        /// <summary>
        /// 禁用输入
        /// </summary>
        void Disable();

        /// <summary>
        /// 编辑输入按键，自动禁用输入和改键完成后自动启用
        /// </summary>
        /// <param name="actionName">操作名称</param>
        /// <param name="bindingName">绑定名称</param>
        ReeditOperation EditInput(string actionName, string bindingName);

        /// <summary>
        /// 编辑输入按键，自动禁用输入和改键完成后自动启用
        /// </summary>
        /// <param name="actionName">操作名称</param>
        /// <param name="bindIndex">绑定索引</param>
        ReeditOperation EditInput(string actionName, int bindIndex);
        
        /// <summary>
        /// 获取所有激活的操作名称
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetActiveActions();
        
        /// <summary>
        /// 获取指定操作的所有单绑定名称，不包含复合绑定
        /// </summary>
        /// <param name="actionName"></param>
        /// <returns></returns>
        IEnumerable<BindingInfo> GetCurrentBindings(string actionName);

        /// <summary>
        /// 获取指定操作的指定索引绑定
        /// </summary>
        /// <param name="actionName"></param>
        /// <param name="bindingIndex"></param>
        /// <returns></returns>
        BindingInfo GetCurrentBinding(string actionName, int bindingIndex);

        /// <summary>
        /// 切换控制方案
        /// </summary>
        /// <param name="scheme"></param>
        void SwitchScheme(string scheme);
    }
}