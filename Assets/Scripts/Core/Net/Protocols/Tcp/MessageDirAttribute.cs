using System;

namespace Core.Net.Protocols.Tcp
{
    internal enum EMessageHandle
    {
        Send,
        Resolve,
        Both,
    }
    
    /// <summary>
    /// 消息的发送方向特性
    /// </summary>
    internal class MessageDirAttribute : Attribute
    {
        public EMessageHandle MessageHandle { get; }
        
        public MessageDirAttribute(EMessageHandle handle)
        {
            MessageHandle = handle;
        }
    }
}
