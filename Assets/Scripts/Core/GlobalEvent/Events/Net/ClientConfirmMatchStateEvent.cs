namespace Core.GlobalEvent.Events.Net
{
    /// <summary>
    /// 通知所有客户端某个客户端确认状态的事件
    /// </summary>
    public class ClientConfirmMatchStateEvent : Event
    {
        public int ConfirmMatchClient { get; set; }
        
        public bool ConfirmMatchState { get; set; }
    }
}
