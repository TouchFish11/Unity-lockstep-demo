using System;
using System.Threading.Tasks;
using Core.DI;
using Core.Log;
using UnityEngine.U2D;

namespace Core.AssetBundles.Management
{
    /// <summary>
    /// 资源加载器
    /// </summary>
    internal static class AssetLoader
    {
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
            if (entry.assetType == EAssetType.Sprite)
            {
                // TODO: 若是图片，则默认直接加载图片资源，即暂时不处理打包到图集的情况，后续在处理
                Logger.Log($"{nameof(AssetLoader)}: loading sprite asset {entry.assetName}");
                
                // // 加载图集
                // var atlasWrapper = await wrapper.LoadAssetAsync<T>(entry.assetName);
                // // 图集不为null
                // if (!atlasWrapper.IsNull)
                // {
                //     // 加载Sprite
                //     var sprite = (atlasWrapper.Asset as SpriteAtlas)?.GetSprite(entry.spriteName);
                //     return DIContainer.Create<AssetWrapper>(parameterValues: new object[] { sprite, wrapper});
                // }
                // return DIContainer.Create<AssetWrapper>();
            }

            // 该资源是其它类型，直接加载返回即可
            var assetWrapper = await wrapper.LoadAssetAsync<T>(entry.assetName);
            return assetWrapper;
        }
    }
}
