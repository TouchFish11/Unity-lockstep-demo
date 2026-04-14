using System;

namespace Core.AssetBundles.Management
{
    /// <summary>
    /// 资源定位
    /// </summary>
    internal class AssetLocation
    {
        /// <summary>
        /// 资源
        /// </summary>
        internal object Asset { get; set; } 
            
        /// <summary>
        /// 当前有效版本号
        /// </summary>
        internal int Version { get; set; }
            
        /// <summary>
        /// 资源引用计数
        /// </summary>
        internal int RefCount { get; set; }
            
        /// <summary>
        /// 资源释放回调
        /// </summary>
        internal Action release;
        
        // ...
    }
}
