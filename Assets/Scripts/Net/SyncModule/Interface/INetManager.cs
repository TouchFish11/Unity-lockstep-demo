using System;
using Net.Protocols;
using Net.SyncModule.Manager;

namespace Net.SyncModule.Interface
{
    /// <summary>
    /// 网络管理器接口
    /// </summary>
    public interface INetManager
    {
        event Action<Message, EProtocolChannel> OnMessageReceived;
        event Action OnConnected;
        
        event Action OnDisconnected;
        
        event Action<string> OnError;
        
        void Connect();
        
        void Send(Message message, EProtocolChannel channel);

        void Disconnect();
    }
}
