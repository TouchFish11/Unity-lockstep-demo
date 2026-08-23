namespace Core.Net.Protocols
{
    /// <summary>
    /// 网络错误码
    /// </summary>
    public enum EErrorCode
    {
        // 兼容Kcp----------------------
        
        /// <summary>
        /// 无法解析主机名
        /// </summary>
        DnsResolve,       
        
        /// <summary>
        /// ping 超时或连接断开
        /// </summary>
        Timeout,
        
        /// <summary>
        /// 消息数量超过传输层/网络处理能力
        /// </summary>
        Congestion,
        
        /// <summary>
        /// 接收无效数据包（可能是故意攻击）
        /// </summary>
        InvalidReceive,
        
        /// <summary>
        /// 用户尝试发送无效数据
        /// </summary>
        InvalidSend,      
        
        /// <summary>
        /// 连接被主动关闭或意外丢失
        /// </summary>
        ConnectionClosed,
        
        /// <summary>
        /// 非预期错误/异常，需要修复
        /// </summary>
        Unexpected,        
        
        // ----------------------------
        
        /// <summary>
        /// 连接到目标服务器失败
        /// </summary>
        ConnectServerFail,    
        
        /// <summary>
        /// 消息数据解析异常
        /// </summary>
        DataResolve,
    }
}
