using System.Collections.Generic;
using Core.GlobalEvent;
using Core.Net.Protocols.FSync;

namespace Core.Net.Events
{
    public class FrameCommandsEvent : Event
    {
        public List<InputCommand> Commands { get; set; }
    }
}
