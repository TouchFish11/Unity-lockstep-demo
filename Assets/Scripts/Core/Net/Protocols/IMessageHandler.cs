using System;

namespace Core.Net.Protocols
{
    /// <summary>
    /// 消息处理器
    /// </summary>
    public interface IMessageHandler
    {
        /// <summary>
        /// 消息类型
        /// </summary>
        Type MessageType { get; }
        
        /// <summary>
        /// 处理消息
        /// </summary>
        /// <param name="message"></param>
        void HandleMessage(Message message);
    }
}
