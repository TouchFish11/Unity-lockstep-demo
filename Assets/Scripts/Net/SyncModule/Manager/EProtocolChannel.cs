namespace Net.SyncModule.Manager
{
    /// <summary>
    /// 协议通道
    /// </summary>
    public enum EProtocolChannel : byte
    {
        /// <summary>
        /// 处理有自定义消息头的消息类型，比如非帧同步使用的消息
        /// </summary>
        Reliable = 1,
        
        /// <summary>
        /// 处理没有自定义消息头的消息类型，比如帧同步使用的消息
        /// </summary>
        Unreliable = 2
    }
}
