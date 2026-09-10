using Core.GlobalEvent;

namespace Core.Net.Events
{
    /// <summary>
    /// 比赛结束事件（胜负已定）
    /// </summary>
    public class RaceEndEvent : Event
    {
        /// <summary>
        /// true=胜利 false=失败
        /// </summary>
        public bool Win { get; set; }
    }
}