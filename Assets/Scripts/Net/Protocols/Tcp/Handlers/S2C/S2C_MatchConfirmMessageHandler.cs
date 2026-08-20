using Net.Protocols.Tcp.Messages.Battle.C2S;

namespace Net.Protocols.Tcp.Handlers.S2C
{
    public class S2C_MatchConfirmMessageHandler : MessageHandler<MatchConfirmMessage>
    {
        public override MatchConfirmMessage Message { get; protected set; }
    
        protected override void OnHandle()
        {

        }
    }
}
