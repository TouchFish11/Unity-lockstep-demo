namespace Core.GlobalEvent.Events.Net
{
    /// <summary>
    /// 其它玩家客户端连入服务器事件
    /// </summary>
    public class OtherPlayerJoinEvent : Event
    {
        public int OtherClientId { get; set; }
    }
}
