using System;
using Core.DI;
using Core.Mono;
using kcp2k;
using Net.Protocols;
using Net.SyncModule.Clients;
using Net.SyncModule.Interface;
using KcpClient = Net.SyncModule.Clients.KcpClient;

namespace Net.SyncModule.Manager
{
    /// <summary>
    /// 网络管理器
    /// </summary>
    internal partial class NetManager : INetManager
    {
        [Inject] private IMonoAdapter _monoAdapter;
        
        /// <summary>
        /// 默认配置
        /// </summary>
        private static readonly NetConfig DefaultConfig = new()
        {
            Resolver = MessageSerializerGetter.BinaryMessageSerializer(),
            ClientType = EClientType.Kcp,
            KcpConfig = new KcpConfig()
        };
        
        // 客户端
        private readonly IProtocolClient _client;
        // 消息序列化器
        private readonly IMessageResolver _messageResolver;
        // 当前网络配置
        private readonly NetConfig _config;
        
        public event Action<Message, EProtocolChannel> OnMessageReceived;
        public event Action OnConnected;
        public event Action OnDisconnected;
        public event Action<string> OnError;
        
        public NetManager(NetConfig config)
        {
            if (config == null) 
                throw new ArgumentNullException(nameof(config));
            
            // 初始化
            _messageResolver = config.Resolver ?? DefaultConfig.Resolver;
            _client = config.ClientType switch
            {
                EClientType.Dual => new DualChannelClient(),
                EClientType.Kcp => new KcpClient(config.KcpConfig ?? DefaultConfig.KcpConfig),
                EClientType.Tcp => new TcpClient(),
                _ => throw new ArgumentOutOfRangeException()
            };
            
            _client.OnConnected += () => OnConnected?.Invoke();
            _client.OnDisconnected += () => OnDisconnected?.Invoke();
            _client.OnError += error => OnError?.Invoke(error);
            _client.OnDataReceived += OnDataReceived;
            _config = config;
            
            _monoAdapter.AddUpdateListener(OnUpdate);
        }

        /// <summary>
        /// 连接到指定IP和端口的服务器
        /// </summary>
        public void Connect()
        {
            _client.Connect(_config.ServerIp, _config.ServerPort);
        }

        public void Send(Message message, EProtocolChannel channel)
        {
            var messageBytes = _messageResolver.Serialize(message, channel);
            _client.SendAsync(messageBytes, channel);
        }

        private void OnDataReceived(byte[] msgData, EProtocolChannel channel)
        {
            OnMessageReceived?.Invoke(_messageResolver.Deserialize(msgData, channel), channel);
        }

        public void OnUpdate()
        {
            _client.Tick();
        }

        public void Disconnect()
        {
            _client.Disconnect();
            _monoAdapter.RemoveUpdateListener(OnUpdate);
        }
    }
}
