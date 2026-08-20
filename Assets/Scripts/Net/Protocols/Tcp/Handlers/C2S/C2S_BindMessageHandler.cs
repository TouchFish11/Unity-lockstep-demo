using Net.Protocols.Tcp.Messages.Common;

namespace Net.Protocols.Tcp.Handlers.C2S
{
    public class C2S_BindMessageHandler : MessageHandler<C2S_BindMessage>
    {
        public override C2S_BindMessage Message { get; protected set; }
    
        protected override void OnHandle()
        {
            
        }
    }
}
