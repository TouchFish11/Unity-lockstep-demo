using System.Threading.Tasks;
using Core.AssetBundles.Management;
using Core.DI;
using Core.Global.Configs;
using Core.GlobalEvent;
using Core.GlobalEvent.Events.Net;
using Core.Inputs;
using Core.Log;
using Core.Math;
using Core.Net.Protocols;
using Core.Net.Protocols.Tcp;
using Core.Net.Protocols.Tcp.Messages.Battle.C2S;
using Core.Net.SyncModule.Interface;
using Core.Net.SyncModule.Manager;
using Core.UI;
using Core.UI.ViewController;
using HotUpdate.Game.Race.Logic;
using HotUpdate.Game.Race.View;
using HotUpdate.UI.Loading;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Logger = Core.Log.Logger;

namespace HotUpdate.UI
{
    public class MainController : UIController<MainView>
    {
        [Inject] private INetManager _netManager;
        [Inject] private ObjectSpawner _objectSpawner;
        [Inject] private IUIManager _uiManager;
        [Inject] private NetGameManager _netGameManager;
        [Inject] private IInputSystem _iInputSystem;
        
        // 是否正在匹配
        private bool isMatching;
        // 是否已经连接服务器
        private bool _isConnected;
        // 逻辑世界
        private LogicWorld logicWorld;
        private ConfirmPanelUI _confirmPanelUI;

        
        protected override Task OnInit()
        {
            var config = GlobalSettings.Instance.netModuleConfig.netConfig;
            _netManager.Init(config);
            return Task.CompletedTask;
        }
        
        protected override Task OnActive()
        {
            _netManager.OnConnected += OnConnected;
            _netManager.OnDisconnected += OnDisconnected;
            ((IHeartbeatService)_netManager).OnRttCalc += OnRtt;
            eventCenter.SubscribeEvent<PlayerDisconnectedEvent>(PlayerDisconnected);
            eventCenter.SubscribeEvent<MatchSuccessEvent>(MatchSuccess);
            eventCenter.SubscribeEvent<PrepareRaceEvent>(PrepareRace);
            eventCenter.SubscribeEvent<StartRaceEvent>(StartRace);
            eventCenter.SubscribeEvent<OtherPlayerJoinEvent>(OtherPlayerJoin);
            return Task.CompletedTask;
        }

        protected override Task OnInactivate()
        {
            _netManager.OnConnected -= OnConnected;
            _netManager.OnDisconnected -= OnDisconnected;
            ((IHeartbeatService)_netManager).OnRttCalc -= OnRtt;
            eventCenter.UnsubscribeEvent<PlayerDisconnectedEvent>(PlayerDisconnected);
            eventCenter.UnsubscribeEvent<MatchSuccessEvent>(MatchSuccess);
            eventCenter.UnsubscribeEvent<PrepareRaceEvent>(PrepareRace);
            eventCenter.UnsubscribeEvent<StartRaceEvent>(StartRace);
            eventCenter.UnsubscribeEvent<OtherPlayerJoinEvent>(OtherPlayerJoin);
            return Task.CompletedTask;
        }
        
        protected override void OnButtonClick(string btnName)
        {
            switch (btnName)
            {
                case nameof(view.btnMatch):
                    StartMatch();
                    break;
                case nameof(view.btnConnect):
                    ConnectServer();
                    break;
            }
        }
        
        private void StartMatch()
        {
            var matchMessage = new C2S_MatchMessage
            {
                SessionID = _netManager.SessionId,
                MatchRequest = !isMatching
            };

            _netManager.Send(matchMessage, EProtocolChannel.Resolve);
            isMatching = !isMatching;
            view.btnMatch.GetComponentInChildren<Text>().text = isMatching ? "取消匹配" : "开始匹配";
        }

        private void ConnectServer()
        {
            if (_isConnected)
            {
                view.btnConnect.enabled = false;
                _netManager.Disconnect();
                return;
            }
            
            view.btnConnect.enabled = false;
            _netManager.Connect();
        }

        private async void OnConnected(ConnectResult connectResult)
        {
            view.SetSelfClientId(connectResult.SessionId);
            foreach (var id in connectResult.SessionIds)
            {
                if (!view.ConnectPlayers.TryGetValue(id, out _) && id != connectResult.SessionId)
                {
                    var playerObjUI = await _objectSpawner.SpawnAsync<ConnectPlayerObjUI>(AssetKeys.ConnectPlayerObjUI, view.svOnline.content);
                    playerObjUI.Init(id);
                    view.ConnectPlayers.Add(id, playerObjUI);
                }
            }

            _isConnected = true;
            view.btnConnect.enabled = true;
            view.btnConnect.GetComponentInChildren<Text>().text = "断开服务器";
            Logger.LogDebug(ELogTags.System, $"[Net] 已初始化客户端ID:{connectResult.SessionId}");

            if (connectResult.RaceExist)
            {
                // 初始化场景等
                await PrepareRaceAsync(_netGameManager.RaceId.Value, connectResult.RaceIds);
                // 隐藏主界面
                await _uiManager.SetViewActive(panelId, false);
                // 隐藏加载界面
                var controller = _uiManager.GetController<LoadingController>();
                await _uiManager.DestroyView(controller.PanelId);
                
                // 显示提示UI
                // 正在重新连接到到比赛...

                var reconnectRaceEvent = EventSource.Get<RequestReconnectRaceEvent>();
                eventCenter.TriggerEvent(reconnectRaceEvent);
            }
            else
            {
                // 清理
                _netGameManager.ClearLocalCache();
            }
        }
        
        private void OnDisconnected()
        {
            _isConnected = false;
            ClearUI();
            view.btnConnect.enabled = true;
            view.btnConnect.GetComponentInChildren<Text>().text = "连接服务器";
            Logger.LogDebug(ELogTags.Network, $"[Net] 网络连接已断开");
        }

        private void OnRtt(long rtt)
        {
            view.SetTcpRtt(rtt);
        }

        private async void OtherPlayerJoin(OtherPlayerJoinEvent otherPlayerJoinEvent)
        {
            var playerObjUI = await _objectSpawner.SpawnAsync<ConnectPlayerObjUI>(AssetKeys.ConnectPlayerObjUI, view.svOnline.content);
            playerObjUI.Init(otherPlayerJoinEvent.OtherClientId);
            view.ConnectPlayers.Add(otherPlayerJoinEvent.OtherClientId, playerObjUI);
        }

        private void PlayerDisconnected(PlayerDisconnectedEvent playerDisconnectedEvent)
        {
            if (view.ConnectPlayers.Remove(playerDisconnectedEvent.DisconnectionId, out var playerObjUI))
            {
                _objectSpawner.Release(playerObjUI);
            }
        }

        private async void MatchSuccess(MatchSuccessEvent matchSuccessEvent)
        {
            _confirmPanelUI = await _objectSpawner.SpawnAsync<ConfirmPanelUI>(AssetKeys.ConfirmView, _uiManager.GetLayer(E_UILayer.Mid));
            _confirmPanelUI.Init(matchSuccessEvent.MatchPlayerCount, () =>
            {
                _objectSpawner.Release(_confirmPanelUI);
                _confirmPanelUI = null;
            });
        }

        private async void PrepareRace(PrepareRaceEvent prepareRaceEvent)
        {
            await PrepareRaceAsync(prepareRaceEvent.RaceId, prepareRaceEvent.RaceClientIds);
            _netManager.Send(new C2S_ReadyMessage(), EProtocolChannel.Resolve);
        }

        public async Task PrepareRaceAsync(int selfRaceId, int[] raceIds)
        {
            // 创建加载界面
            await _uiManager.CreateViewAsync<LoadingView, LoadingController>(AssetKeys.LoadingPanel, E_UILayer.Bot);
            // 创建游戏界面
            var raceController = await _uiManager.CreateViewAsync<RaceView, RaceController>(AssetKeys.GameView, E_UILayer.Mid);
            logicWorld = DIContainer.Create<LogicWorld>();
            foreach (var raceClientId in raceIds)
            {
                var logicAvatar = new LogicAvatar(raceClientId, Fixed64.FromFloat(3f));
                logicWorld.AddAvatar(logicAvatar);

                using var handle = await GameAsset.LoadAssetAsync<RuntimeAnimatorController>(AssetKeys.Role1_Animator);
                var viewAvatar = await _objectSpawner.SpawnAsync<ViewAvatar>(AssetKeys.Role1);
                viewAvatar.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                viewAvatar.Bind(logicAvatar, handle.Asset);
                // 自身客户端角色，添加输入
                if (selfRaceId == raceClientId)
                {
                    // 添加输入组件
                    var playerInput = viewAvatar.gameObject.AddComponent<PlayerInput>();
                    viewAvatar.InitInput(_iInputSystem, playerInput);
                    _netGameManager.SetCurrentRaceId(selfRaceId);
                }
                _netGameManager.AddPlayer(raceClientId, viewAvatar);
                // 创建UI
                await raceController.CreateHUD(viewAvatar);
            }
            
            await SpawnAi();   // 新增
        }
        
        private async Task SpawnAi()
        {
            const int AiCount = 2;
            for (var i = 0; i < AiCount; i++)
            {
                var aiId = -1000 - i;   // 负 ID，避免和服务器 SessionId 冲突（纯本地确定性实体，不走网络）
                var logicAvatar = new LogicAvatar(aiId, Fixed64.FromFloat(2f));
                logicWorld.AddAvatar(logicAvatar);

                var random = new DeterministicRandom((uint)(10000 + i));   // 种子固定，确定性
                logicWorld.AddAi(new AiController(logicAvatar, random,
                    new FixedVector3(Fixed64.FromFloat(-8f), Fixed64.Zero, Fixed64.FromFloat(-8f)),
                    new FixedVector3(Fixed64.FromFloat(8f),  Fixed64.Zero, Fixed64.FromFloat(8f))));

                using var handle = await GameAsset.LoadAssetAsync<RuntimeAnimatorController>(AssetKeys.Role1_Animator);
                var viewAvatar = await _objectSpawner.SpawnAsync<ViewAvatar>(AssetKeys.AIRole);
                viewAvatar.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                viewAvatar.Bind(logicAvatar, handle.Asset);
            }
        }
        
        private async void StartRace(StartRaceEvent startRaceEvent)
        {
            _objectSpawner.Release(_confirmPanelUI);
            _confirmPanelUI = null;
            // 隐藏主界面
            await _uiManager.SetViewActive(panelId, false);
            // 隐藏加载界面
            var controller = _uiManager.GetController<LoadingController>();
            await _uiManager.DestroyView(controller.PanelId);
        }

        private void ClearUI()
        {
            foreach (var connectPlayerObjUI in view.ConnectPlayers.Values)
            {
                _objectSpawner.Release(connectPlayerObjUI);
            }
            view.ConnectPlayers.Clear();
        }
        
        protected override Task OnDispose()
        {
            ClearUI();
            _objectSpawner.Clear();
            return base.OnDispose();
        }
    }
}
