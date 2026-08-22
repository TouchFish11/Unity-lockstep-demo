using System.Threading.Tasks;
using Core.DI;
using Core.EditorRes;
using Core.GlobalEvent;
using Core.GlobalEvent.Events.Net;
using Core.Log;
using Core.Mono;
using Core.Net.Protocols;
using Core.Net.Protocols.Tcp.Messages.Battle.C2S;
using Core.Net.SyncModule.Interface;
using Core.Net.SyncModule.Manager;
using Core.UI.ViewController;
using kcp2k;
using Net.Protocols;
using UnityEngine;
using UnityEngine.UI;
using Logger = Core.Log.Logger;

namespace HotUpdate.UI.UI
{
    public class MainController : UIController<MainView>
    {
        [Inject] private INetGameProxy _netProxy;
        [Inject] private IEventCenter _eventCenter;
        [Inject] private IEditorResManager _editorResManager;
        
        // 是否正在匹配
        private bool isMatching;
        // 是否已经连接服务器
        private bool _isConnected;
        
        protected override Task OnInit()
        {
            _eventCenter.SubscribeEvent<PlayerDisconnectedEvent>(PlayerDisconnected);
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
                _netProxy.Disconnect();
                _isConnected = false;
                view.btnConnect.GetComponentInChildren<Text>().text = "连接服务器";
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
            
            var proxy = DIContainer.Resolve<INetGameProxy>().Init(config);
            proxy.OnConnected += OnConnected;
            proxy.OnDisconnected += OnDisconnected;
            proxy.TcpRtt += rtt => view.SetTcpRtt(rtt);
            proxy.Connect();
            
            view.btnConnect.GetComponentInChildren<Text>().text = "断开服务器";
        }

        private void OnConnected(int clientId, int[] clientIds)
        {
            view.SetSelfClientId(clientId);
            
            foreach (var id in clientIds)
            {
                if (!view.ConnectPlayers.TryGetValue(clientId, out _))
                {
                    var uiObj = _editorResManager.LoadEditorAsset<GameObject>("");
                    var playerObjUI = uiObj.GetComponent<ConnectPlayerObjUI>();
                    playerObjUI.transform.SetParent(view.svOnline.content, false);
                    playerObjUI.Init(id);
                    view.ConnectPlayers.Add(id, playerObjUI);
                }
            }
            
            Logger.LogDebug(ELogTags.System, $"[Net] 已初始化客户端ID:{clientId}");
        }
        
        private void OnDisconnected()
        {
            Logger.LogDebug(ELogTags.Network, $"[Net] 网络连接已断开");
        }

        private void PlayerDisconnected(PlayerDisconnectedEvent playerDisconnectedEvent)
        {
            if (view.ConnectPlayers.Remove(playerDisconnectedEvent.DisconnectionId, out var playerObjUI))
            {
                EngineUtility.Destroy(playerObjUI.gameObject);
            }
        }

        protected override Task OnDispose()
        {
            _netProxy.OnConnected -= OnConnected;
            _eventCenter.UnsubscribeEvent<PlayerDisconnectedEvent>(PlayerDisconnected);
            
            foreach (var connectPlayerObjUI in view.ConnectPlayers.Values)
            {
                EngineUtility.Destroy(connectPlayerObjUI.gameObject);
            }
            view.ConnectPlayers.Clear();
            
            return base.OnDispose();
        }
    }
}
