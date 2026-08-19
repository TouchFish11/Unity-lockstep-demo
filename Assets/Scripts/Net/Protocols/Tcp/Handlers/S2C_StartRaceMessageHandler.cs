using Net.Protocols.Tcp.Messages.Battle.S2C;

namespace Net.Protocols.Tcp.Handlers
{
    public class S2C_StartRaceMessageHandler : MessageHandler<S2C_StartRaceMessage>
    {
        public override S2C_StartRaceMessage Message { get; protected set; }
        
        protected override void OnHandle()
        {
            
        }
    }
}
