namespace Core.AssetBundles.Management
{
    /// <summary>
    /// 图片定位对象
    /// </summary>
    internal class SpriteLocation : AssetLocation
    {
        /// <summary>
        /// 该图集的子图片资源key
        /// </summary>
        internal string SpriteKey { get; set; }
    }
}
