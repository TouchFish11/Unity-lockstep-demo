using System.Threading.Tasks;
using Core.DI;
using UnityEngine.U2D;

namespace Core.AssetBundles.Management
{
    /// <summary>
    /// 资源加载器
    /// </summary>
    internal static class AssetLoader
    {
        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="wrapper"></param>
        /// <param name="entry"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static AssetWrapper LoadAsset<T>(BundleWrapper wrapper, AssetEntry entry) where T : UnityEngine.Object
        {
            // 该资源是图片
            if (entry is SpriteAssetEntry spriteEntry)
            {
                // 加载图集资源
                var atlasWrapper = wrapper.LoadAsset<T>(spriteEntry.spriteAssetName);
                // 图集为null
                if (atlasWrapper.IsNull) 
                    return DIContainer.Create<AssetWrapper>();
                
                // 加载Sprite资源，当前key就是图片在该图集的名称，被打到图集的图片不能通过资源路径加载
                var sprite = (atlasWrapper.Asset as SpriteAtlas)?.GetSprite(spriteEntry.key);
                return DIContainer.Create<AssetWrapper>(parameterValues: new object[] { sprite, wrapper});
            }
            
            // 该资源是其它类型，直接加载返回即可
            var assetWrapper = wrapper.LoadAsset<T>(entry.assetName);
            return assetWrapper;
        }

        private static AssetWrapper LoadTexture2D<T>(AssetWrapper atlasWrapper, SpriteAssetEntry spriteEntry) where T : UnityEngine.Object
        {
            // 图集为null
            if (atlasWrapper.IsNull) 
                return DIContainer.Create<AssetWrapper>();
                
            // 加载Sprite资源，当前key就是图片在该图集的名称，被打到图集的图片不能通过资源路径加载
            var sprite = ((SpriteAtlas)atlasWrapper.Asset).GetSprite(spriteEntry.key);
            return DIContainer.Create<AssetWrapper>(parameterValues: new object[] { sprite, atlasWrapper});
        }

        private static AssetWrapper LoadNonTexture2D(BundleWrapper wrapper, SpriteAssetEntry spriteEntry)
        {
            
        }
        
        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="wrapper"></param>
        /// <param name="entry"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async Task<AssetWrapper> LoadAssetAsync<T>(BundleWrapper wrapper, AssetEntry entry) where T : class
        {
            // 该资源是图片
            if (entry is SpriteAssetEntry spriteEntry)
            {
                // 加载图集资源
                var atlasWrapper = await wrapper.LoadAssetAsync<T>(spriteEntry.spriteAssetName);
                // 图集为null
                if (atlasWrapper.IsNull) 
                    return DIContainer.Create<AssetWrapper>();
                
                // 加载Sprite资源，当前key就是图片在该图集的名称，被打到图集的图片不能通过资源路径加载
                var sprite = (atlasWrapper.Asset as SpriteAtlas)?.GetSprite(spriteEntry.key);
                return DIContainer.Create<AssetWrapper>(parameterValues: new object[] { sprite, wrapper});
            }
            
            // 该资源是其它类型，直接加载返回即可
            var assetWrapper = await wrapper.LoadAssetAsync<T>(entry.assetName);
            return assetWrapper;
        }
    }
}
