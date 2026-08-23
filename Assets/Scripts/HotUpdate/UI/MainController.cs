using System.Threading.Tasks;
using Core.AssetBundles.Management;
using Core.DI;
using Core.GlobalEvent;
using Core.GlobalEvent.Events.Net;
using Core.Log;
using Core.Net.Protocols;
using Core.Net.Protocols.Tcp.Messages.Battle.C2S;
using Core.Net.SyncModule.Interface;
using Core.Net.SyncModule.Manager;
using Core.Scene;
using Core.UI;
using Core.UI.ViewController;
using HotUpdate.Game.Race;
using HotUpdate.UI.Loading;
using kcp2k;
using Net.Protocols;
using UnityEngine.SceneManagement;
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
        [Inject] private ISceneManager _sceneManager;
        [Inject] private NetGameManager _netGameManager;
        
        // 是否正在匹配
        private bool isMatching;
        // 是否已经连接服务器
        private bool _isConnected;
        private ConfirmPanelUI _confirmPanelUI;
        
        protected override Task OnInit()
        {
            _eventCenter.SubscribeEvent<PlayerDisconnectedEvent>(PlayerDisconnected);
            _eventCenter.SubscribeEvent<MatchSuccessEvent>(MatchSuccess);
            _eventCenter.SubscribeEvent<PrepareRaceEvent>(PrepareRace);
            return Task.CompletedTask;
        }
        
        protected override Task OnActive()
        {
            return Task.CompletedTask;
        }

        protected override Task OnInactivate()
        {
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
            
            var config = new NetConfig
            {
                ServerIp = "127.0.0.1",
                ServerPort = 8080,
                Resolver = MessageSerializerSource.DefaultMessageResolver,
                ClientType = EClientType.Tcp,
                KcpConfig = new KcpConfig(DualMode:false, Timeout: 30000)
            };
                        
            view.btnConnect.enabled = false;
            _netProxy.Init(config);
            _netProxy.OnConnected += OnConnected;
            _netProxy.OnDisconnected += OnDisconnected;
            _netProxy.TcpRtt += rtt => view.SetTcpRtt(rtt);
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
            view.btnConnect.GetComponentInChildren<Text>().text = "连接服务器";
            Logger.LogDebug(ELogTags.Network, $"[Net] 网络连接已断开");
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
            var loading = await _uiManager.CreateViewAsync<LoadingView, LoadingController>(AssetKeys.LoadingPanel, E_UILayer.Bot);
            _objectSpawner.Release(_confirmPanelUI);
            _confirmPanelUI = null;

            await _sceneManager.LoadSceneAsync("", LoadSceneMode.Single, null);

            foreach (var raceClientId in prepareRaceEvent.RaceClientIds)
            {
                var roleController = await _objectSpawner.SpawnAsync<RaceRoleController>(AssetKeys.RolePrefab);

                if (_netProxy.ClientId == raceClientId)
                {
                    // TODO：添加输入组件；添加相机
                    // InputComponent inputComponent = playerCharacter.AddComponent<InputComponent>();
                    // GameObject mainCamera = AssetBundleLoadManager.Instance.LoadAsset<GameObject>(E_AssetBundleType.Camera, "Main Camera");
                    // GameObject mainCameraInstance = GameObject.Instantiate(mainCamera);
                    // CameraController cameraController = mainCameraInstance.GetComponent<CameraController>();
                    // cameraController.Init(playerCharacter.transform);
                    // inputComponent.enabled = false;
                }
                
                roleController.Init(raceClientId);
                _netGameManager.AddPlayer(raceClientId, roleController);
            }
            
            _netProxy.Send(new C2S_ReadyMessage(), EProtocolChannel.Resolve);
        }
        
        protected override Task OnDispose()
        {
            _netProxy.OnConnected -= OnConnected;
            _eventCenter.UnsubscribeEvent<PlayerDisconnectedEvent>(PlayerDisconnected);
            
            foreach (var connectPlayerObjUI in view.ConnectPlayers.Values)
            {
                _objectSpawner.Release(connectPlayerObjUI);
            }
            _objectSpawner.Clear();
            view.ConnectPlayers.Clear();
            
            return base.OnDispose();
        }
    }
}
