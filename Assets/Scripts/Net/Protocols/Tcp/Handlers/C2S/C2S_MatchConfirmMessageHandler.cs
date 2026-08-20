using Net.Protocols.Tcp.Messages.Battle.C2S;

namespace Net.Protocols.Tcp.Handlers.C2S
{
    public class C2S_MatchConfirmMessageHandler : MessageHandler<MatchConfirmMessage>
    {
        public override MatchConfirmMessage Message { get; protected set; }
    
        protected override void OnHandle()
        {
            
        }
    }
}
