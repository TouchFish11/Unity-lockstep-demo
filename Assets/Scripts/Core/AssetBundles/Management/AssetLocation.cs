namespace Core.AssetBundles.Management
{
    /// <summary>
    /// 资源定位
    /// </summary>
    internal class AssetLocation
    {
        /// <summary>
        /// 物理资源键或组合资源键
        /// </summary>
        internal string AssetKey { get; set; }
        
        /// <summary>
        /// 当前有效版本号
        /// </summary>
        internal int Version { get; set; }
    }
}
