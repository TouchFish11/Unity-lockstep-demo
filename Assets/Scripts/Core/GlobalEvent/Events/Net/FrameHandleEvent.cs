namespace Core.GlobalEvent.Events.Net
{
    /// <summary>
    /// 处理了每帧后触发该事件
    /// </summary>
    public class FrameHandleEvent : Event
    {
        /// <summary>
        /// 当前客户端处理过的最新帧
        /// </summary>
        public int FrameId { get; set; }
    }
}
