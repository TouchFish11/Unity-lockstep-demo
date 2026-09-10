using Core.GlobalEvent;
using Core.Net.Events;
using Core.Net.Protocols.Tcp.Messages.Common;

namespace Core.Net.Protocols.Tcp.Handlers.S2C
{
    public class S2C_ClientJoinMessageHandler : MessageHandler<S2C_ClientJoinMessage>
    {
        public override S2C_ClientJoinMessage Message { get; protected set; }
    
        protected override void OnHandle()
        {
            var otherPlayerJoinEvent = EventSource.Get<OtherPlayerJoinEvent>();
            otherPlayerJoinEvent.OtherClientId = Message.SessionID;
            eventCenter.TriggerEvent(otherPlayerJoinEvent);
        }
    }
}
