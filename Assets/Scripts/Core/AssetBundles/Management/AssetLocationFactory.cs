using Core.DI;
using UnityEngine;

namespace Core.AssetBundles.Management
{
    /// <summary>
    /// 资源定位对象工厂
    /// </summary>
    internal class AssetLocationFactory
    {
        /// <summary>
        /// 获取组合键的资源定位对象
        /// </summary>
        /// <param name="combineKey"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public static AssetLocation GetAssetLocationCombine(string combineKey, int version)
        {
            var assetLocation = DIContainer.Create<AssetLocation>();
            assetLocation.AssetKey = combineKey;
            assetLocation.Version = version;
            return assetLocation;
        }
        
        /// <summary>
        /// 获取资源定位对象
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="version"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static AssetLocation GetAssetLocation<T>(AssetEntry entry, int version) where T : class
        {
            if (typeof(Sprite) == typeof(T) && entry is SpriteAssetEntry spriteAssetEntry)
            {
                var spriteLocation = DIContainer.Create<spriteLocation>();
                spriteLocation.AssetKey = spriteAssetEntry.atlasKey;
                spriteLocation.Version = version;
                spriteLocation.SpriteKey = spriteAssetEntry.key;
                return spriteLocation;
            }
            
            var assetLocation = DIContainer.Create<AssetLocation>();
            assetLocation.AssetKey = entry.key;
            assetLocation.Version = version;
            return assetLocation;
        }
    }
}
