using Core.DI;
using Core.GlobalEvent;
using Core.GlobalEvent.Events.Net;
using Core.Net.Protocols.Tcp.Messages.Battle.S2C;

namespace Core.Net.Protocols.Tcp.Handlers.S2C
{
    public class S2C_ClientConfirmMatchStateMessageHandler : MessageHandler<S2C_ClientConfirmMatchStateMessage>
    {
        [Inject] private IEventCenter _eventCenter;
        
        public override S2C_ClientConfirmMatchStateMessage Message { get; protected set; }
    
        protected override void OnHandle()
        {
            var clientConfirmMatchStateEvent = EventSource.Get<ClientConfirmMatchStateEvent>();
            clientConfirmMatchStateEvent.ConfirmMatchClient = Message.SessionID;
            clientConfirmMatchStateEvent.ConfirmMatchState = Message.CurrentConfirmMatchState;
            _eventCenter.TriggerEvent(clientConfirmMatchStateEvent);
        }
    }
}
