namespace Core.DI
{
    /// <summary>
    /// 构造参数结构
    /// </summary>
    public struct ParameterArg
    {
        /// <summary>
        /// 参数名
        /// </summary>
        public string ArgName { get; set; }
        
        /// <summary>
        /// 参数值
        /// </summary>
        public object ArgValue { get; set; }
    }
}
