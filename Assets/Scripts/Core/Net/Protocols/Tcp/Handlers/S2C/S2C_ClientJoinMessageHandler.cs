using Net.Protocols.Tcp.Messages.Common;

namespace Net.Protocols.Tcp.Handlers.S2C
{
    public class S2C_ClientJoinMessageHandler : MessageHandler<S2C_ClientJoinMessage>
    {
        public override S2C_ClientJoinMessage Message { get; protected set; }
    
        protected override void OnHandle()
        {
            
        }
    }
}
