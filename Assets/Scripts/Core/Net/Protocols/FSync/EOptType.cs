namespace Core.Net.Protocols.FSync
{
    /// <summary>
    /// 帧操作消息的操作类型
    /// </summary>
    public enum EOptType : byte
    {
        /// <summary>
        /// 无（占位）
        /// </summary>
        None=0,
        
        /// <summary>
        /// 移动
        /// </summary>
        Move=1,
        
        /// <summary>
        /// 攻击
        /// </summary>
        Attack=2,
        
        /// <summary>
        /// 技能
        /// </summary>
        UseSkill=3,
    }
}
