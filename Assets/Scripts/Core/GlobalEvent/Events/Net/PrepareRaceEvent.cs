namespace Core.GlobalEvent.Events.Net
{
    public class PrepareRaceEvent : Event
    {
        public int[] RaceClientIds { get; set; }
    }
}
