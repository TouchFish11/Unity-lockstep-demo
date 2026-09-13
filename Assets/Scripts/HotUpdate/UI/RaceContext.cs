using System.Collections.Generic;
using System.Threading.Tasks;
using Core.AssetBundles.Management;
using Core.DI;
using Core.GlobalEvent;
using Core.Inputs;
using Core.Math;
using Core.Net.Events;
using Core.Net.Protocols;
using Core.Net.Protocols.Tcp;
using Core.Net.Protocols.Tcp.Messages.Common;
using Core.Net.SyncModule.Interface;
using Core.Net.SyncModule.Manager;
using Core.UI;
using Game.Vfx;
using HotUpdate.Game.Race.Logic;
using HotUpdate.Game.Race.Present;
using HotUpdate.Game.Race.View;
using HotUpdate.UI.Loading;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HotUpdate.UI
{
    /// <summary>
    /// 一场比赛的上下文：封装逻辑世界 + 玩家/AI 表现 + HUD 的完整生命周期
    /// </summary>
    public class RaceContext
    {
        private readonly IEventCenter _eventCenter;
        private readonly ObjectSpawner _objectSpawner;
        private readonly NetGameManager _netGameManager;
        private readonly INetManager _netManager;
        private readonly IInputSystem _inputSystem;
        private readonly IUIManager _uiManager;

        private LogicWorld _logicWorld;
        private readonly List<ViewAvatar> _viewAvatars = new();
        // effectId → 资源 key
        private readonly Dictionary<int, string> _effectKeyMap = new()
        {
            {0, AssetKeys.Attack1},
            { 1, AssetKeys.AoeAttack },
        }; 
        
        public ERaceState State { get; private set; } = ERaceState.None;
        public LogicWorld World => _logicWorld;

        public RaceContext(IEventCenter eventCenter, ObjectSpawner objectSpawner, NetGameManager netGameManager, IInputSystem inputSystem, IUIManager uiManager, INetManager netManager)
        {
            _eventCenter = eventCenter;
            _objectSpawner = objectSpawner;
            _netGameManager = netGameManager;
            _netManager = netManager;
            _inputSystem = inputSystem;
            _uiManager = uiManager;
        }
        
        /// <summary>
        /// 构建这一局的逻辑世界 + 玩家/AI 表现 + HUD
        /// </summary>
        public async Task PrepareAsync(int selfRaceId, int[] raceIds)
        {
            State = ERaceState.Preparing;

            // 加载界面 + 游戏界面
            await _uiManager.CreateViewAsync<LoadingView, LoadingController>(AssetKeys.LoadingPanel, E_UILayer.Bot);
            var raceController = await _uiManager.CreateViewAsync<RaceView, RaceController>(AssetKeys.GameView, E_UILayer.Mid);

            // 逻辑世界
            _logicWorld = DIContainer.Create<LogicWorld>();
            ReplayRecorder.Start(raceIds);   // 开始录制本局

            // 玩家（逻辑 + 表现 + HUD）
            foreach (var raceId in raceIds)
            {
                var logicAvatar = new LogicAvatar(raceId, CharacterTable.Get(CharacterTable.PlayerId));
                _logicWorld.AddAvatar(logicAvatar);

                using var handle = await GameAsset.LoadAssetAsync<RuntimeAnimatorController>(AssetKeys.Role1_Animator);
                var viewAvatar = await _objectSpawner.SpawnAsync<ViewAvatar>(AssetKeys.Role1);
                viewAvatar.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                viewAvatar.Bind(logicAvatar, handle.Asset);
                _viewAvatars.Add(viewAvatar);

                if (selfRaceId == raceId)
                {
                    var playerInput = viewAvatar.gameObject.AddComponent<PlayerInput>();
                    viewAvatar.InitInput(_inputSystem, playerInput);
                    _netGameManager.SetCurrentRaceId(selfRaceId);
                }
                _netGameManager.AddPlayer(raceId, viewAvatar);

                // HUD 是 UI 显示，交给 RaceController（它内部用 view.transform 做父节点）
                await raceController.CreateHUD(viewAvatar);
            }

            // AI
            await SpawnAi();
            
            // 监听连接事件：比赛期间的同进程重连由这里处理（MainController 隐藏后不再监听）
            _netManager.OnConnected += OnConnected;
            // 监听比赛结束
            _eventCenter.SubscribeEvent<RaceEndEvent>(OnRaceEnd);
            // 监听特效播放
            _eventCenter.SubscribeEvent<PresentEventsEvent>(OnPresentEvents);
        }
        
        /// <summary>
        /// 比赛开始：销毁加载界面，切到 Playing
        /// </summary>
        public async Task StartRace()
        {
            State = ERaceState.Playing;
            var controller = _uiManager.GetController<LoadingController>();
            await _uiManager.DestroyView(controller.PanelId);
        }
        
        /// <summary>
        /// 重连：触发追帧流程
        /// </summary>
        public void Reconnect()
        {
            // 显示提示UI
            // 正在重新连接到到比赛...
            
            State = ERaceState.Reconnecting;
            var evt = EventSource.Get<RequestReconnectRaceEvent>();
            _eventCenter.TriggerEvent(evt);
        }
        
        /// <summary>
        /// 比赛期间连接事件：同进程重连（服务器仍有比赛）时触发追帧
        /// </summary>
        private void OnConnected(ConnectResult connectResult)
        {
            if (connectResult.RaceExist)
            {
                Reconnect();
            }
        }
        
        /// <summary>
        /// 离开比赛：退订逻辑世界，释放表现对象
        /// </summary>
        public void Leave()
        {
            if (State == ERaceState.Ended)
                return;
            
            State = ERaceState.Ended;
            _logicWorld?.Unsubscribe();
            _netManager.OnConnected -= OnConnected;
            _eventCenter.UnsubscribeEvent<RaceEndEvent>(OnRaceEnd);
            _eventCenter.UnsubscribeEvent<PresentEventsEvent>(OnPresentEvents);
            _objectSpawner.Release(_viewAvatars, true);
        }
        
        /// <summary>
        /// 比赛结束：返回主界面并清理比赛
        /// </summary>
        private async void OnRaceEnd(RaceEndEvent evt)
        {
            if (State == ERaceState.Ended)
                return;
            
            // 通知服务器比赛结束，允许重新匹配
            _netManager.Send(new C2S_RaceEndMessage(), EProtocolChannel.Resolve);
            ReplayRecorder.Stop();   // 停止录制，保留供回放

            // 关闭战斗界面（RaceView，会顺带释放 HUD）
            var raceController = _uiManager.GetController<RaceController>();
            if (raceController != null)
            {
                await _uiManager.DestroyView(raceController.PanelId);
            }
            
            // 显示结果面板，点"返回大厅"才回主界面
            var resultPanel = await _objectSpawner.SpawnAsync<ResultPanelUI>(AssetKeys.ResultView, _uiManager.GetLayer(E_UILayer.Mid));
            resultPanel.Init(evt.Win, () =>
            {
                _objectSpawner.Release(resultPanel);
                ReturnToLobby();
            });
        }
        
        private async void ReturnToLobby()
        {
            // 返回主界面
            var mainController = _uiManager.GetController<MainController>();
            if (mainController != null)
                await _uiManager.SetViewActive(mainController.PanelId, true);

            // 清理比赛
            Leave();
        }
        
        private async void OnPresentEvents(PresentEventsEvent evt)
        {
            foreach (var e in evt.Events.ToArray())
            {
                if (e.Type != EPresentEventType.SpawnEffect)
                    continue;
                if (!_effectKeyMap.TryGetValue(e.Id, out var key) || string.IsNullOrEmpty(key))
                    continue;
                var timer = await _objectSpawner.SpawnAsync<VfxTimer>(key, null, e.Pos.ToVector3(), Quaternion.identity);
                timer.overCallback += () =>
                {
                    _objectSpawner.Release(timer);
                };
            }
        }
        
        private async Task SpawnAi()
        {
            const int AiCount = 1;
            for (var i = 0; i < AiCount; i++)
            {
                var aiId = -1000 - i;
                var logicAvatar = new LogicAvatar(aiId, CharacterTable.Get(CharacterTable.MonsterId));
                _logicWorld.AddAvatar(logicAvatar);

                var random = new DeterministicRandom((uint)(10000 + i));
                _logicWorld.AddAi(new AiController(logicAvatar));

                using var handle = await GameAsset.LoadAssetAsync<RuntimeAnimatorController>(AssetKeys.Role1_Animator);
                var viewAvatar = await _objectSpawner.SpawnAsync<ViewAvatar>(AssetKeys.AIRole);
                viewAvatar.transform.SetPositionAndRotation(new Vector3(5, i * 3, 0), Quaternion.identity);
                viewAvatar.Bind(logicAvatar, handle.Asset);
                _viewAvatars.Add(viewAvatar);
                
                // HUD 是 UI 显示，交给 RaceController（它内部用 view.transform 做父节点）
                await _uiManager.GetController<RaceController>().CreateHUD(viewAvatar);
            }
        }
    }
}
