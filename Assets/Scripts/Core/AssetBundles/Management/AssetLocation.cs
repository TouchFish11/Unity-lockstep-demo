using System;

namespace Core.AssetBundles.Management
{
    /// <summary>
    /// 资源定位
    /// </summary>
    internal class AssetLocation
    {
        // // 资源
        // private object _asset;
        //
        // /// <summary>
        // /// 资源
        // /// </summary>
        // internal object Asset
        // {
        //     get => _asset;
        //     set
        //     {
        //         _asset = value;
        //         access?.Invoke();
        //     }
        // }
        //
        
        /// <summary>
        /// 资源包装器
        /// </summary>
        internal AssetWrapper AssetWrapper { get; set; }
            
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
    }
}
