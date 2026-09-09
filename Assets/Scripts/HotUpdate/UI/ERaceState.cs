namespace HotUpdate.UI
{
    public enum ERaceState : byte
    {
        None = 0,
        Preparing,      // 构建场景中
        Playing,        // 游戏中
        Reconnecting,   // 重连中
        Ended,          // 已结束/已离开
    }
}