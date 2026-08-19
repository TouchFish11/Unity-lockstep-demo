using Net.Protocols.Tcp.Messages.Common;

namespace Net.Protocols.Tcp.Handlers
{
    /// <summary>
    /// 服务器连接消息处理器
    /// </summary>
    public class S2C_ConnectMessageHandler : MessageHandler<S2C_ConnectMessage>
    {
        public override S2C_ConnectMessage Message { get; protected set; }
        
        protected override void OnHandle()
        {
            // 记录ID
            
            // 发送心跳
        }
    }
}
