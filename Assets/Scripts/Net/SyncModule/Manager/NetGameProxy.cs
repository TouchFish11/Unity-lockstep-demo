using System;
using Core.DI;
using Core.Log;
using Net.Protocols;
using Net.Protocols.FSync.Messages;
using Net.Protocols.Tcp;
using Net.SyncModule.Interface;

namespace Net.SyncModule.Manager
{
    /// <summary>
    /// 网络游戏代理
    /// </summary>
    public class NetGameProxy : INetGameProxy
    {
        // 封装底层网络管理器
        [Inject] private INetManager _netManager;
        
        // 消息路由处理
        private MessageRouter _router;

        public event Action<int> OnGameConnected;
        
        public event Action OnGameDisconnected;

        public event Action<long> TcpRtt;

        /// <summary>
        /// 服务器下发的当前连接的客户端ID
        /// </summary>
        public int SessionId { get; private set; }

        private NetGameProxy()
        {
            
        }
        
        public INetGameProxy Init(NetConfig netConfig)
        {
            _netManager = DIContainer.Create<NetManager>(parameterValues: netConfig);
            _netManager.OnConnected += OnGameConnectedInternal;
            _netManager.OnMessageReceived += OnMessageReceive;
            ((IHeartbeatService)_netManager).OnRttCalc += TcpRtt;
            _router = DIContainer.Create<MessageRouter>();
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
            if(channel == EProtocolChannel.Resolve)
                ((TcpMessage)message).SessionID = SessionId;
            else
                ((C2S_NextFrameMessage)message).OptMessage.SessionID = SessionId;
            _netManager.Send(message, channel);
        }

        public void Disconnect()
        {
            _netManager.Disconnect();
        }

        private void OnGameConnectedInternal()
        {
            Logger.LogDebug(ELogTags.System,$"[Net Connected] 连接服务器成功!");
        }

        private void OnMessageReceive(Message message, EProtocolChannel channel)
        {
            _router.Dispatch(message);
        }
    }
}
