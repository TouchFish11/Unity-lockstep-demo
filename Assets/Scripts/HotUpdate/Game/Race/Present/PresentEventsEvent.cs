using System.Collections.Generic;
using Core.GlobalEvent;

namespace HotUpdate.Game.Race.Present
{
    public class PresentEventsEvent : Event
    {
        public readonly List<PresentEvent> Events = new();

        public override void ResetEvent() => Events.Clear();
    }
}