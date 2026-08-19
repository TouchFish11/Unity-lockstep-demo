using Net.Protocols.Tcp.Messages.Common;

namespace Net.Protocols.Tcp.Handlers
{
    public class S2C_HeartMessageHandler : MessageHandler<HeartMessage>
    {
        public override HeartMessage Message { get; protected set; }
        
        protected override void OnHandle()
        {
            // 计算rtt

        }
    }
}
