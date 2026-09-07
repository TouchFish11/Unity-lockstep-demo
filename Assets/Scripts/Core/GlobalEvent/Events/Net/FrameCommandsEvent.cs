using System.Collections.Generic;
using Core.Net.Protocols.FSync;

namespace Core.GlobalEvent.Events.Net
{
    public class FrameCommandsEvent : Event
    {
        public List<InputCommand> Commands { get; set; }
    }
}
