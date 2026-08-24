using System;
using Core.Net.Protocols.Tcp.Messages.Common;
using Net.Protocols;

namespace Core.Net.Protocols.Tcp.Handlers.C2S
{
    public class C2S_HeartMessageHandler : MessageHandler<HeartMessage>
    {
        public override HeartMessage Message { get; protected set; }
    
        protected override void OnHandle()
        {
            Message.ServerTimeStamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }
}
