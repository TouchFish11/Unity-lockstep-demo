using System;
using Core.DI;
using Core.Mono;
using Core.Net.Protocols;
using Core.Net.Protocols.FSync.Messages;
using Core.Net.Protocols.Tcp;
using Core.Net.SyncModule.Interface;

namespace Core.Net.SyncModule.Manager
{
    /// <summary>
    /// 网络游戏代理
    /// </summary>
    public class NetGameProxy : INetGameProxy, IApplicationExitNotify
    {
        // 封装底层网络管理器
        [Inject] private INetManager _netManager;
        
        // 消息路由处理
        private MessageRouter _router;

        public int QuitPriority => 1;
        
        public int ClientId => _netManager.SessionId;
        
        public event Action<int, int[]> OnConnected;
        
        public event Action OnDisconnected;

        public event Action<long> TcpRtt;
        
        private NetGameProxy(IMonoAdapter monoAdapter)
        {
            monoAdapter.AddApplicationExitNotify(this);
        }
        
        public INetGameProxy Init(NetConfig netConfig)
        {
            _netManager = DIContainer.Resolve<NetManager>();
            _netManager.Init(netConfig);
            _netManager.OnConnected += OnConnectedEvent;
            _netManager.OnMessageReceived += OnMessageReceive;
            _netManager.OnDisconnected += OnDisconnectedEvent;
            _netManager.OnError += OnError;
            ((IHeartbeatService)_netManager).OnRttCalc += RttCalcEvent;
            _router = DIContainer.Create<MessageRouter>();
            return this;
        }
    
        // 游戏层调用接口
        public void Connect()
        {
            _netManager.Connect();
        }
        
        public void Send(Message message, EProtocolChannel channel)
        {
            if(channel == EProtocolChannel.Resolve && message is TcpMessage tcpMessage)
                tcpMessage.SessionID = _netManager.SessionId;
            _netManager.Send(message, channel);
        }

        public void Disconnect()
        {
            _netManager.Disconnect();
        }

        private void OnConnectedEvent(int clientId, int[] clientIds)
        {
            OnConnected?.Invoke(clientId, clientIds);
        }

        private void OnDisconnectedEvent()
        {
            OnDisconnected?.Invoke();
        }

        private void OnMessageReceive(Message message, EProtocolChannel channel)
        {
            _router.Dispatch(message);
        }

        private void RttCalcEvent(long rttMs)
        {
            TcpRtt?.Invoke(rttMs);
        }
        
        private void OnError(EErrorCode code, string message)
        {
            Disconnect();
        }
        
        public void OnAppQuit()
        {
            Disconnect();
        }
    }
}
