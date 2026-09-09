using System.Collections.Generic;
using System.Threading.Tasks;
using Core.AssetBundles.Management;
using Core.DI;
using Core.GlobalEvent;
using Core.GlobalEvent.Events.Net;
using Core.Inputs;
using Core.Math;
using Core.Net.SyncModule.Manager;
using Core.UI;
using HotUpdate.Game.Race.Logic;
using HotUpdate.Game.Race.View;
using HotUpdate.UI.Loading;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HotUpdate.UI
{
    /// <summary>
    /// 一场比赛的上下文：封装逻辑世界 + 玩家/AI 表现 + HUD 的完整生命周期
    /// </summary>
    public class RaceContext : MonoBehaviour
    {
        private readonly IEventCenter _eventCenter;
        private readonly ObjectSpawner _objectSpawner;
        private readonly NetGameManager _netGameManager;
        private readonly IInputSystem _inputSystem;
        private readonly IUIManager _uiManager;

        private LogicWorld _logicWorld;
        private readonly List<ViewAvatar> _viewAvatars = new();
        
        public ERaceState State { get; private set; } = ERaceState.None;
        public LogicWorld World => _logicWorld;

        public RaceContext(IEventCenter eventCenter, ObjectSpawner objectSpawner, NetGameManager netGameManager, IInputSystem inputSystem, IUIManager uiManager)
        {
            _eventCenter = eventCenter;
            _objectSpawner = objectSpawner;
            _netGameManager = netGameManager;
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

            // 玩家（逻辑 + 表现 + HUD）
            foreach (var raceId in raceIds)
            {
                var logicAvatar = new LogicAvatar(raceId, Fixed64.FromFloat(3f));
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

            // 4. AI
            await SpawnAi();
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
            State = ERaceState.Reconnecting;
            var evt = EventSource.Get<RequestReconnectRaceEvent>();
            _eventCenter.TriggerEvent(evt);
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
            _objectSpawner.Release(_viewAvatars);
        }
        
        private async Task SpawnAi()
        {
            const int AiCount = 2;
            for (var i = 0; i < AiCount; i++)
            {
                var aiId = -1000 - i;
                var logicAvatar = new LogicAvatar(aiId, Fixed64.FromFloat(2f));
                _logicWorld.AddAvatar(logicAvatar);

                var random = new DeterministicRandom((uint)(10000 + i));
                _logicWorld.AddAi(new AiController(logicAvatar, random,
                    new FixedVector3(Fixed64.FromFloat(-8f), Fixed64.Zero, Fixed64.FromFloat(-8f)),
                    new FixedVector3(Fixed64.FromFloat(8f), Fixed64.Zero, Fixed64.FromFloat(8f))));

                using var handle = await GameAsset.LoadAssetAsync<RuntimeAnimatorController>(AssetKeys.Role1_Animator);
                var viewAvatar = await _objectSpawner.SpawnAsync<ViewAvatar>(AssetKeys.AIRole);
                viewAvatar.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                viewAvatar.Bind(logicAvatar, handle.Asset);
                _viewAvatars.Add(viewAvatar);
            }
        }
    }
}
