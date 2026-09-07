using System.Collections.Generic;
using System.Threading.Tasks;
using Core.AssetBundles.Management;
using Core.DI;
using Core.GlobalEvent;
using Core.GlobalEvent.Events.Net;
using Core.Inputs;
using Core.Log;
using Core.Math;
using Core.Net.Protocols;
using Core.Net.Protocols.Tcp.Messages.Battle.C2S;
using Core.Net.SyncModule.Interface;
using Core.Net.SyncModule.Manager;
using Core.UI;
using Core.UI.ViewController;
using HotUpdate.Game.Race;
using HotUpdate.Game.Race.Logic;
using HotUpdate.Game.Race.View;
using HotUpdate.UI.Loading;
using kcp2k;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Logger = Core.Log.Logger;

namespace HotUpdate.UI
{
    public class MainController : UIController<MainView>
    {
        [Inject] private INetGameProxy _netProxy;
        [Inject] private IEventCenter _eventCenter;
        [Inject] private ObjectSpawner _objectSpawner;
        [Inject] private IUIManager _uiManager;
        [Inject] private NetGameManager _netGameManager;
        [Inject] private IInputSystem _iInputSystem;
        
        // 是否正在匹配
        private bool isMatching;
        // 是否已经连接服务器
        private bool _isConnected;
        private ConfirmPanelUI _confirmPanelUI;

        private readonly List<StatusHUD> _huds = new();
        
        protected override Task OnInit()
        {
            var config = new NetConfig
            {
                ServerIp = "127.0.0.1",
                ServerPort = 8080,
                Resolver = MessageSerializerSource.DefaultMessageResolver,
                ClientType = EClientType.Kcp,
                KcpConfig = new KcpConfig(Timeout: 15000)
            };
            _netProxy.Init(config);
            return Task.CompletedTask;
        }
        
        protected override Task OnActive()
        {
            _netProxy.OnConnected += OnConnected;
            _netProxy.OnDisconnected += OnDisconnected;
            _netProxy.TcpRtt += rtt => view.SetTcpRtt(rtt);
            _eventCenter.SubscribeEvent<PlayerDisconnectedEvent>(PlayerDisconnected);
            _eventCenter.SubscribeEvent<MatchSuccessEvent>(MatchSuccess);
            _eventCenter.SubscribeEvent<PrepareRaceEvent>(PrepareRace);
            _eventCenter.SubscribeEvent<StartRaceEvent>(StartRace);
            _eventCenter.SubscribeEvent<OtherPlayerJoinEvent>(OtherPlayerJoin);
            return Task.CompletedTask;
        }

        protected override Task OnInactivate()
        {
            _netProxy.OnConnected -= OnConnected;
            _netProxy.OnDisconnected -= OnDisconnected;
            _netProxy.TcpRtt -= OnRtt;
            _eventCenter.UnsubscribeEvent<PlayerDisconnectedEvent>(PlayerDisconnected);
            _eventCenter.UnsubscribeEvent<MatchSuccessEvent>(MatchSuccess);
            _eventCenter.UnsubscribeEvent<PrepareRaceEvent>(PrepareRace);
            _eventCenter.UnsubscribeEvent<StartRaceEvent>(StartRace);
            _eventCenter.UnsubscribeEvent<OtherPlayerJoinEvent>(OtherPlayerJoin);
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
                SessionID = _netProxy.ClientId,
                MatchRequest = !isMatching
            };

            _netProxy.Send(matchMessage, EProtocolChannel.Resolve);
            isMatching = !isMatching;
            view.btnMatch.GetComponentInChildren<Text>().text = isMatching ? "取消匹配" : "开始匹配";
        }

        private void ConnectServer()
        {
            if (_isConnected)
            {
                view.btnConnect.enabled = false;
                _netProxy.Disconnect();
                return;
            }
            
            view.btnConnect.enabled = false;
            _netProxy.Connect();
        }

        private async void OnConnected(int clientId, int[] clientIds)
        {
            view.SetSelfClientId(clientId);
            foreach (var id in clientIds)
            {
                if (!view.ConnectPlayers.TryGetValue(clientId, out _) && id != clientId)
                {
                    var playerObjUI = await _objectSpawner.SpawnAsync<ConnectPlayerObjUI>(AssetKeys.ConnectPlayerObjUI, view.svOnline.content);
                    playerObjUI.Init(id);
                    view.ConnectPlayers.Add(id, playerObjUI);
                }
            }

            _isConnected = true;
            view.btnConnect.enabled = true;
            view.btnConnect.GetComponentInChildren<Text>().text = "断开服务器";
            Logger.LogDebug(ELogTags.System, $"[Net] 已初始化客户端ID:{clientId}");
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
            // 创建加载界面
            await _uiManager.CreateViewAsync<LoadingView, LoadingController>(AssetKeys.LoadingPanel, E_UILayer.Mid);
            // 创建游戏界面
            var raceController = await _uiManager.CreateViewAsync<RaceView, RaceController>(AssetKeys.GameView, E_UILayer.Bot);
            
            var logicWorld = new LogicWorld(_eventCenter);
            
            foreach (var raceClientId in prepareRaceEvent.RaceClientIds)
            {
                var logicAvatar = new LogicAvatar(raceClientId, Fixed64.FromFloat(3f));
                logicWorld.AddAvatar(logicAvatar);
                
                var viewAvatar = await _objectSpawner.SpawnAsync<ViewAvatar>(AssetKeys.RolePrefab);
                viewAvatar.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                viewAvatar.Bind(logicAvatar);
                
                // 自身客户端角色，添加输入
                var self = _netProxy.ClientId == raceClientId;
                if (self)
                {
                    // 添加输入组件
                    var playerInput = viewAvatar.gameObject.AddComponent<PlayerInput>();
                    viewAvatar.InitInput(_iInputSystem, playerInput);
                }
                
                _netGameManager.AddPlayer(raceClientId, viewAvatar);
                
                // 创建UI
                var statusHUD = await _objectSpawner.SpawnAsync<StatusHUD>(AssetKeys.StatusHUD);
                statusHUD.Init(viewAvatar, raceController.View);
                _huds.Add(statusHUD);
            }
            
            _netProxy.Send(new C2S_ReadyMessage(), EProtocolChannel.Resolve);
        }

        private async void StartRace(StartRaceEvent startRaceEvent)
        {
            // 隐藏确认UI，主界面
            _objectSpawner.Release(_confirmPanelUI);
            _confirmPanelUI = null;
            await _uiManager.SetViewActive(panelId, false);
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
            _objectSpawner.Release(_huds);
            _objectSpawner.Clear();
            return base.OnDispose();
        }
    }
}
