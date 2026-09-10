using System;
using Core.Net.Protocols;
using Core.Net.Protocols.Tcp;

namespace Core.Net.SyncModule.Interface
{
    /// <summary>
    /// 网络管理器接口
    /// </summary>
    public interface INetManager
    {
        /// <summary>
        /// 网络连接到服务器回调
        /// </summary>
        event Action<ConnectResult> OnConnected;
        
        /// <summary>
        /// 与服务器断开连接回调
        /// </summary>
        event Action OnDisconnected;
        
        /// <summary>
        /// 网络错误回调
        /// </summary>
        event Action<EErrorCode, string> OnError;

        /// <summary>
        /// 服务器下发的当前连接的客户端ID，仅在当前连接有效，不可跨会话
        /// </summary>
        int SessionId { get; }

        /// <summary>
        /// 是否处于连接状态
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// 初始化网络配置，不能重复调用，全局仅调用一次
        /// </summary>
        /// <param name="config"></param>
        void Init(NetConfig config);
        
        /// <summary>
        /// 连接到服务器
        /// </summary>
        void Connect();
        
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="channel"></param>
        void Send(Message message, EProtocolChannel channel);

        /// <summary>
        /// 主动断开连接
        /// </summary>
        void Disconnect();
    }
}
