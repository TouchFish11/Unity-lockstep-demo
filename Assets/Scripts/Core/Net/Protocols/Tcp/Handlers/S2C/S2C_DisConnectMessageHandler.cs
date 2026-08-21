using Net.Protocols.Tcp.Messages.Common;

namespace Net.Protocols.Tcp.Handlers.S2C
{
    public class S2C_DisConnectMessageHandler : MessageHandler<S2C_DisConnectMessage>
    {
        public override S2C_DisConnectMessage Message { get; protected set; }
    
        protected override void OnHandle()
        {

        }
    }
}
