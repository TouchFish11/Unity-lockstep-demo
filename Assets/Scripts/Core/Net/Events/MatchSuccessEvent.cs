using Core.GlobalEvent;

namespace Core.Net.Events
{
    /// <summary>
    /// 匹配到比赛成功事件
    /// </summary>
    public class MatchSuccessEvent : Event
    {
        public int MatchPlayerCount { get; set; }
    }
}
