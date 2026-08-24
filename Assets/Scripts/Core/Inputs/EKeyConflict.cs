namespace Core.Inputs
{
    /// <summary>
    /// 键位冲突类型
    /// </summary>
    public enum EKeyConflict : byte
    {
        /// <summary>
        /// 特殊键位冲突
        /// </summary>
        SpecialKey,
        
        /// <summary>
        /// 相同按键冲突
        /// </summary>
        ExistKey,
        
        /// <summary>
        /// 无冲突
        /// </summary>
        Over,
    }
}