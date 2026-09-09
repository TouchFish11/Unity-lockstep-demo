using Core.GlobalEvent;
using Core.GlobalEvent.Events.Net;
using Core.Net.Protocols.Tcp.Messages.Battle.S2C;

namespace Core.Net.Protocols.Tcp.Handlers.S2C
{
    public class S2C_PrepareRaceMessageHandler : MessageHandler<S2C_PrepareRaceMessage>
    {
        public override S2C_PrepareRaceMessage Message { get; protected set; }
        
        protected override void OnHandle()
        {
            var prepareRaceEvent = EventSource.Get<PrepareRaceEvent>();
            prepareRaceEvent.RaceId = Message.RaceID;
            prepareRaceEvent.RaceClientIds = Message.clientIds.ToArray();
            eventCenter.TriggerEvent(prepareRaceEvent);
        }
    }
}
