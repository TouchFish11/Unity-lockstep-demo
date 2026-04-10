using System.Collections.Generic;
using System.Threading.Tasks;
using Core.AssetBundles.Management;
using Core.DI;
using Core.Log;
using UnityEngine.U2D;

namespace Core.Loader.Sprite
{
    /// <summary>
    /// 精灵图片加载器
    /// 负责从SpriteAtlas（精灵图集）中异步加载指定名称的Sprite（精灵图片）
    /// 当图集或指定精灵加载失败时，返回默认精灵（当前默认返回null）
    /// </summary>
    public class SpriteLoader : ISpriteLoader
    {
        // AB包管理器接口
        private readonly IAssetBundleManager _assetBundleManager = DIContainer.GetInstance<IAssetBundleManager>();
        // 图集缓存
        private readonly Dictionary<string, AtlasData> _atlasDatas =  new();
        
        public async Task<UnityEngine.Sprite> LoadSpriteAsync(string abName, string atlasName, string assetName)
        {
            // 存在图集
            if (_atlasDatas.TryGetValue(atlasName, out var atlasData))
            {
                // 存在Sprite
                if (atlasData.TryGetSprite(assetName, out var cacheSprite))
                {
                    return cacheSprite;
                }
                
                // 从图集中获取指定名称的精灵
                var sprite = atlasData.Atlas.GetSprite(assetName);
                // 缓存Sprite
                if (sprite)
                {
                    atlasData.TryAdd(assetName, sprite);
                    return sprite;
                }

                Logger.LogWarning($"{nameof(SpriteLoader)}.{nameof(LoadSpriteAsync)}，{abName}.{atlasName}.{assetName}资源获取失败，返回默认Sprite");
                return null;
            }
            else
            {
                var handle = await GameAsset.LoadAssetAsync<SpriteAtlas>(atlasName);
                
                // 图集加载失败，则返回默认精灵
                if (!handle.Asset)
                {
                    Logger.LogWarning($"{nameof(SpriteLoader)}.{nameof(LoadSpriteAsync)}，{abName}.{atlasName}图集加载失败，返回默认Sprite");
                    return null;
                }
                
                // 缓存图集
                var newAtlasData = new AtlasData(handle.Asset);
                if (!_atlasDatas.TryAdd(atlasName, newAtlasData))
                {
                    Logger.LogWarning($"{nameof(AtlasData)}.{nameof(LoadSpriteAsync)}：重复缓存{abName}中的SpriteAtlas，{atlasName}");
                }
                
                // 图集加载成功，从图集中获取指定名称的精灵
                var sprite = handle.Asset.GetSprite(assetName);
                if (sprite)
                {
                    newAtlasData.TryAdd(assetName, sprite);
                    return sprite;
                }
            
                Logger.LogWarning($"{nameof(SpriteLoader)}.{nameof(LoadSpriteAsync)}，{abName}.{atlasName}.{assetName}资源获取失败，返回默认Sprite");
                return null;
            }
        }
        
        public void ReleaseSprite(string abName, string atlasName, string spriteName)
        {
            if (!_atlasDatas.TryGetValue(atlasName, out var atlasData))
            {
                return;
            }
            
            // 卸载图片
            atlasData.Unload(spriteName);
            if(atlasData.GetRefCount() == 0)
            {
                _atlasDatas.Remove(atlasName);
                _assetBundleManager.UnloadBundle(abName);
            }
        }
    }
}