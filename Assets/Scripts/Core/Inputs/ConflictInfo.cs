using UnityEngine.InputSystem;

namespace Core.Inputs
{
    /// <summary>
    /// 冲突信息结构
    /// </summary>
    internal struct ConflictInfo
    {
        /// <summary>
        /// 当前重绑定Action原来的绑定
        /// </summary>
        public InputBinding CurrentBinding { get; set; }
        
        /// <summary>
        /// 当前重绑定Action原来的绑定索引
        /// </summary>
        public int CurrentBindingIndex { get; set; }
        
        /// <summary>
        /// 其它Action已经占有的绑定的索引
        /// </summary>
        public int ConflictBindingIndex { get; set; }
        
        /// <summary>
        /// 其它Action已经占有的绑定
        /// </summary>
        public InputBinding ConflictBinding { get; set; }
    }
}
