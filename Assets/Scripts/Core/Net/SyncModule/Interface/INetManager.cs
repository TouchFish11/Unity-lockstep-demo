using System;
using Core.Global.Configs;
using Core.Net.Protocols;
using Core.Net.Protocols.Tcp;

namespace Core.Net.SyncModule.Interface
{
    /// <summary>
    /// 网络管理器接口
    /// </summary>
    public interface INetManager
    {
        event Action<ConnectResult> OnConnected;
        
        event Action OnDisconnected;
        
        event Action<EErrorCode, string> OnError;

        /// <summary>
        /// 服务器下发的当前连接的客户端ID，仅在当前连接有效，不可跨会话
        /// </summary>
        int SessionId { get; }

        void Init(NetConfig config);
        
        void Connect();
        
        void Send(Message message, EProtocolChannel channel);

        void Disconnect();
    }
}
