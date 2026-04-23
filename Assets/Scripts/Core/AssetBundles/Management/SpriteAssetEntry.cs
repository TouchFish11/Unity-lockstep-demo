using Newtonsoft.Json;

namespace Core.AssetBundles.Management
{
    [JsonObject(MemberSerialization.OptIn)]
    public class SpriteAssetEntry : AssetEntry
    {
        /// 图集资源名称，该图片所在的图集，图集的资源路径，用于加载图集资源
        [JsonProperty] public string spriteAssetName;
        
        /// <summary>
        /// 图片资源条目构造函数
        /// </summary>
        /// <param name="key">图片资源名</param>
        /// <param name="bundleName">图片所在的图集包名，即图集包的名称</param>
        /// <param name="assetName">图片资源本身的资源路径，不会去用这个路径加载图片，除非不打图集</param>
        /// <param name="assetType">资源的类型，是Texture</param>
        /// <param name="spriteAssetName">图集资源名称，该图片所在的图集，图集的资源路径，用于加载图集资源</param>
        public SpriteAssetEntry(string key, string bundleName, string assetName, EAssetType assetType
        , string spriteAssetName) : base(key, bundleName, assetName, assetType)
        {
            this.spriteAssetName = spriteAssetName;
            // 通过key 获取到图片entry
            
            // 图片entry的包名就是图集包 -> 加载图集包
            
            // 加载图集资源 -> 图片entry的图集路径就是图集资源
            
            // key 就是图片资源名称（图集内的名称）
            
            // 返回图片
        }
    }
}
