namespace Core.Net.Protocols.Tcp
{
    /// <summary>
    /// 连接结果
    /// </summary>
    public struct ConnectResult
    {
        /// <summary>
        /// 当前客户端的连接ID
        /// </summary>
        public int SessionId { get; set; }
        
        /// <summary>
        /// 所有已连接的客户端ID，包括发送目标客户端
        /// </summary>
        public int[] SessionIds { get; set; }
        
        /// <summary>
        /// 是否存在比赛尚未结束，存在则处理重连逻辑
        /// </summary>
        public bool RaceExist { get; set; }
        
        /// <summary>
        /// 所有客户端的比赛ID
        /// </summary>
        public int[] RaceIds { get; set; }
    }
}
