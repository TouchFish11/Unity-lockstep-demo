using System;
using Core.Exceptions;
using Core.Log;
using Core.Mono;
using Core.Net.kcp2k.highlevel;
using Core.Net.Protocols;
using Core.Net.Protocols.Tcp;
using Core.Net.SyncModule.Clients;
using Core.Net.SyncModule.Interface;
using KcpClient = Core.Net.SyncModule.Clients.KcpClient;

namespace Core.Net.SyncModule.Manager
{
    /// <summary>
    /// 网络管理器
    /// </summary>
    internal partial class NetManager : INetManager, IApplicationExitNotify
    {
        /// <summary>
        /// 默认配置
        /// </summary>
        private static readonly NetConfig DefaultConfig = new()
        {
            resolver = MessageSerializerSource.DefaultMessageResolver,
            clientType = EClientType.Kcp,
            kcpConfig = new KcpConfig()
        };
        
        private readonly IMonoAdapter _monoAdapter;     // Mono适配器
        private readonly MessageRouter _router;         // 消息路由处理
        private IProtocolClient _client;                // 客户端
        private IMessageResolver _messageResolver;      // 消息序列化器
        private NetConfig _config;                      // 当前网络配置
        
        public event Action<ConnectResult> OnConnected;
        public event Action OnDisconnected;
        public event Action<EErrorCode, string> OnError;
        public int SessionId { get; private set; }
        public bool IsConnected => _client.IsConnected;
        
        private NetManager(IMonoAdapter monoAdapter, MessageRouter messageRouter) : this()
        {
            monoAdapter.AddApplicationExitNotify(this);
            _monoAdapter = monoAdapter;
            _router = messageRouter;
        }

        public void Init(NetConfig config)
        {
            if (config == null)
                throw ExceptionHelper.Throw($"{nameof(config)} is null");
            
            // 初始化
            _messageResolver = config.resolver ?? DefaultConfig.resolver;
            _client = config.clientType switch
            {
                EClientType.Dual => new DualChannelClient(),
                EClientType.Kcp => new KcpClient(config.kcpConfig ?? DefaultConfig.kcpConfig),
                EClientType.Tcp => new TcpClient(),
                _ => throw new ArgumentOutOfRangeException()
            };

            _client.OnDisconnected += Disconnected;
            _client.OnError += Error;
            _client.OnDataReceived += ReceivedData;
            _config = config;
            _router.RegisterHandlers();
            _monoAdapter.AddUpdateListener(OnUpdate);
        }

        /// <summary>
        /// 连接到指定IP和端口的服务器
        /// </summary>
        public void Connect()
        {
            if (_client.IsConnected)
            {
                Logger.LogWarning(ELogTags.Network, "Already connected");
                return;
            }
            
            _client.Connect(_config.serverIp, _config.serverPort);
        }
        
        /// <summary>
        /// 设置网络状态，收到服务器的连接消息时被调用
        /// </summary>
        /// <param name="connectResult"></param>
        internal void SetConnectStatus(ConnectResult connectResult)
        {
            // 获取到ID才去通知业务层连接成功
            SessionId = connectResult.SessionId;
            OnConnected?.Invoke(connectResult);
        }

        public void Send(Message message, EProtocolChannel channel)
        {
            if(channel == EProtocolChannel.Resolve && message is TcpMessage tcpMessage)
                tcpMessage.SessionID = SessionId;
            var messageBytes = _messageResolver.Serialize(message, channel);
            _client.SendAsync(messageBytes, channel);
        }
        
        private void Disconnected()
        {
            OnDisconnected?.Invoke();
        }

        private void Error(EErrorCode code,string msg)
        {
            OnError?.Invoke(code, msg);
        }
        
        private void ReceivedData(byte[] msgData, EProtocolChannel channel)
        {
            var message = _messageResolver.Deserialize(msgData, channel);
            _router.Dispatch(message);
        }

        public void OnUpdate()
        {
            _client.Tick();
        }

        public void Disconnect()
        {
            if (_client != null && _client.IsConnected)
            {
                _client?.Disconnect();
            }
        }

        public int QuitPriority => 1;
        
        public void OnAppQuit()
        {
            Disconnect();
        }
    }
}
