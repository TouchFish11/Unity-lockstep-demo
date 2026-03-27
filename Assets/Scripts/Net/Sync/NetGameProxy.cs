using System;
using Net.Sync.Msg.S2C;

namespace Net.Sync
{
    /// <summary>
    /// 网络游戏代理
    /// </summary>
    public class NetGameProxy : INetGameProxy
    {
        // 封装底层网络管理器
        private INetManager _netManager;
        // 消息路由处理
        private MessageRouter _router;
        // 游戏层关注的事件
        public event Action<int> OnGameConnected;
        public event Action OnGameDisconnected;
        
        /// 服务器下发的当前连接的客户端Token
        public int SessionId { get; private set; }
        
        public INetGameProxy Init(NetConfig netConfig)
        {
            _netManager = new NetManager(netConfig);
            _netManager.OnConnected += OnGameConnectedInternal;
            _netManager.OnMessageReceived += OnMessageReceive;
            _router = new MessageRouter();
            return this;
        }
    
        // 游戏层调用接口
        public void Connect()
        {
            _netManager.Connect();
        }

        public void SetSessionToken(int sessionToken)
        {
            OnGameConnected?.Invoke(sessionToken);
            SessionId = sessionToken;
        }

        public void Send(Message message, EProtocolChannel channel)
        {
            message.SessionID = SessionId;
            _netManager.Send(message, channel);
        }

        public void Disconnect()
        {
            _netManager.Disconnect();
        }

        private void OnGameConnectedInternal()
        {
            // 发送连接消息
            _netManager.Send(new ConnectMessage(), EProtocolChannel.Reliable);
        }

        private void OnMessageReceive(Message message, EProtocolChannel channel)
        {
            _router.Dispatch(message);
        }
    }
}
