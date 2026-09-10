using Core.GlobalEvent;

namespace Core.Net.Events
{
    public class PrepareRaceEvent : Event
    {
        /// <summary>
        /// 当前客户端的比赛ID
        /// </summary>
        public int RaceId { get; set; }
        
        /// <summary>
        /// 所有客户端的比赛ID
        /// </summary>
        public int[] RaceClientIds { get; set; }
    }
}
