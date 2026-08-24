using Core.GlobalEvent;
using Core.GlobalEvent.Events.Net;
using Core.Net.Protocols.Tcp.Messages.Battle.S2C;
using Net.Protocols;

namespace Core.Net.Protocols.Tcp.Handlers.S2C
{
    public class S2C_StartRaceMessageHandler : MessageHandler<S2C_StartRaceMessage>
    {
        public override S2C_StartRaceMessage Message { get; protected set; }
        
        protected override void OnHandle()
        {
            var startRaceEvent = EventSource.Get<StartRaceEvent>();
            eventCenter.TriggerEvent(startRaceEvent);
        }
    }
}
