using UnityEngine;

namespace Core.AssetBundles.Management
{
    /// <summary>
    /// 图集定位对象
    /// </summary>
    internal class AtlasLocation : AssetLocation
    {
        /// <summary>
        /// 该图集的子图片资源
        /// </summary>
        internal Object Texture { get; set; }
        
    }
}
