using System.Threading.Tasks;
using Core.AssetBundles.Management;
using Core.DI;
using Core.Global.Configs;
using Core.Log;
using Core.Net.Events;
using Core.Net.Protocols;
using Core.Net.Protocols.Tcp;
using Core.Net.Protocols.Tcp.Messages.Battle.C2S;
using Core.Net.SyncModule.Interface;
using Core.Net.SyncModule.Manager;
using Core.UI;
using Core.UI.ViewController;
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
        
        // 是否正在匹配
        private bool isMatching;
        // 比赛上下文
        private RaceContext _raceContext;
        
        protected override Task OnInit()
        {
            var config = GlobalSettings.Instance.netModuleConfig.netConfig;
            _netManager.Init(config);
            view.SetTcpRtt(-1);
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
                    RequestServer();
                    break;
            }
        }
        
        /// <summary>
        /// 开始匹配
        /// </summary>
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

        /// <summary>
        /// 主动连接或断开服务器
        /// </summary>
        private void RequestServer()
        {
            if (_netManager.IsConnected)
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
            
            view.btnConnect.enabled = true;
            view.btnConnect.GetComponentInChildren<Text>().text = "断开服务器";
            Logger.LogDebug(ELogTags.System, $"[Net] 已初始化客户端ID:{connectResult.SessionId}");

            if (connectResult.RaceExist)
            {
                // 重启无现有场景重建；判空本地没缓存 RaceId 时无法认领，跳过重连
                if (_raceContext == null && _netGameManager.RaceId is { } raceId)
                {
                    _raceContext = DIContainer.Create<RaceContext>();
                    await _raceContext.PrepareAsync(raceId, connectResult.RaceIds);
                    await _uiManager.SetViewActive(panelId, false);
                    await _raceContext.StartRace();
                    _raceContext.Reconnect();
                }
            }
            else
            {
                // 清理
                _netGameManager.ClearLocalCache();
            }
        }
        
        private void OnDisconnected()
        {
            ClearUI();
            view.btnConnect.enabled = true;
            view.btnConnect.GetComponentInChildren<Text>().text = "连接服务器";
            view.SetTcpRtt(-1);
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
            view.ConfirmPanelUI = await _objectSpawner.SpawnAsync<ConfirmPanelUI>(AssetKeys.ConfirmView, _uiManager.GetLayer(E_UILayer.Mid));
            view.ConfirmPanelUI.Init(matchSuccessEvent.MatchPlayerCount, () =>
            {
                _objectSpawner.Release(view.ConfirmPanelUI);
                view.ConfirmPanelUI = null;
            });
        }

        private async void PrepareRace(PrepareRaceEvent prepareRaceEvent)
        {
            _raceContext = DIContainer.Create<RaceContext>();
            await _raceContext.PrepareAsync(prepareRaceEvent.RaceId, prepareRaceEvent.RaceClientIds);
            _netManager.Send(new C2S_ReadyMessage(), EProtocolChannel.Resolve);
        }
        
        private async void StartRace(StartRaceEvent startRaceEvent)
        {
            _objectSpawner.Release(view.ConfirmPanelUI);
            view.ConfirmPanelUI = null;
            isMatching = !isMatching;
            view.btnMatch.GetComponentInChildren<Text>().text = isMatching ? "取消匹配" : "开始匹配";
            // 隐藏主界面
            await _uiManager.SetViewActive(panelId, false);
            await _raceContext.StartRace();
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
            _raceContext?.Leave();
            return base.OnDispose();
        }
    }
}
