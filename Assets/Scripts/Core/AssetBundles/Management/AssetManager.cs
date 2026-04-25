using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.U2D;
using Object = UnityEngine.Object;

namespace Core.AssetBundles.Management
{
    /// <summary>
    /// 资源管理器
    /// </summary>
    internal class AssetManager
    {
        private readonly IAssetBundleManager _assetBundleManager;
        // 物理资源key到物理资源的缓存映射
        private readonly Dictionary<string, AssetWrapper> _assetWrappers = new();
        // 精灵图片缓存
        private readonly Dictionary<(string atlasKey, string spriteKey), Sprite> _sprites = new();

        public AssetManager(IAssetBundleManager assetBundleManager)
        {
            _assetBundleManager = assetBundleManager;
        }

        /// <summary>
        /// 获取资源条目
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public AssetEntry GetAssetEntry(string key)
        {
            return _assetBundleManager.Catalog.GetEntry(key);
        }
        
        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <param name="key"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public AssetWrapper LoadAsset<T>(string key) where T : Object
        {
            if (_assetWrappers.TryGetValue(key, out var assetWrapper))
            {
                return assetWrapper;
            }
            
            // 从资源目录中查找指定的资源路径
            var mapEntry = _assetBundleManager.Catalog.GetEntry(key);
            if (mapEntry == null)
                throw new NullReferenceException($"{nameof(GameAsset)}: key({key}) found entry is null");
            
            // 加载AB包
            var bundleWrapper = _assetBundleManager.LoadBundle(mapEntry.bundleName);
            if (bundleWrapper == null)
                throw new NullReferenceException($"{nameof(GameAsset)}: load {mapEntry.bundleName} AssetBundle failed");
            
            // 加载资源
            assetWrapper = bundleWrapper.LoadAsset<T>(key, mapEntry.assetName);
            if (assetWrapper.IsNull)
                throw new NullReferenceException($"{nameof(GameAsset)}: load {mapEntry.assetName} failed, key({key})");
            
            _assetWrappers.Add(key, assetWrapper);
            return assetWrapper;
        }
        
        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="key"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<AssetWrapper> LoadAssetAsync<T>(string key) where T : Object
        {
            // 从资源目录中查找指定的资源路径
            var entry = _assetBundleManager.Catalog.GetEntry(key);
            if (entry == null)
                throw new NullReferenceException($"{nameof(GameAsset)}: key({key}) found entry is null");
            
            if (entry is SpriteAssetEntry spriteAssetEntry)
            {
                // 存在该资源缓存，直接返回
                if (_assetWrappers.TryGetValue(spriteAssetEntry.atlasKey, out var assetWrapper))
                {
                    assetWrapper.Retain();
                    return assetWrapper;
                }
                
                // 异步加载指定资源AB包
                var bundleWrapper = await _assetBundleManager.LoadBundleAsync(spriteAssetEntry.bundleName);
                if (bundleWrapper.IsNull)
                    throw new NullReferenceException($"{nameof(GameAsset)}: load {spriteAssetEntry.bundleName} AssetBundle failed");
                
                // 加载图集资源
                assetWrapper = await bundleWrapper.LoadAssetAsync<SpriteAtlas>(spriteAssetEntry.atlasKey, spriteAssetEntry.spriteAssetName);
                // 避免"并发"逻辑上重复添加
                if (_assetWrappers.TryGetValue(spriteAssetEntry.atlasKey, out var cacheWrapper))
                {
                    cacheWrapper.Retain();
                    return cacheWrapper;
                }
                
                assetWrapper.Retain();
                // 缓存资源包装
                _assetWrappers.Add(spriteAssetEntry.atlasKey, assetWrapper);
                return assetWrapper;
            }
            else
            {
                // 存在该资源缓存，直接返回
                if (_assetWrappers.TryGetValue(key, out var assetWrapper))
                {
                    assetWrapper.Retain();
                    return assetWrapper;
                }
                
                // 异步加载指定资源AB包
                var bundleWrapper = await _assetBundleManager.LoadBundleAsync(entry.bundleName);
                if (bundleWrapper.IsNull)
                    throw new NullReferenceException($"{nameof(GameAsset)}: load {entry.bundleName} AssetBundle failed");
                
                // 加载资源
                assetWrapper = await bundleWrapper.LoadAssetAsync<T>(key, entry.assetName);
                // 避免"并发"逻辑上重复添加
                if (_assetWrappers.TryGetValue(key, out var cacheWrapper))
                {
                    cacheWrapper.Retain();
                    return cacheWrapper;
                }
                
                assetWrapper.Retain();
                // 缓存资源包装
                _assetWrappers.Add(key, assetWrapper);
                return assetWrapper;
            }
        }

        /// <summary>
        /// 异步加载指定包的所有资源
        /// </summary>
        /// <param name="bundleName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<AssetWrapper[]> LoadAllAssetAsync<T>(string bundleName) where T : Object
        {
            var allKeys = new List<string>(_assetBundleManager.Catalog.GetAssetKeysByBundle(bundleName));
            var assetToLoadKeys = new List<string>();
            foreach (var assetKey in allKeys)
            {
                // 添加待加载的资源key
                if (!_assetWrappers.ContainsKey(assetKey))
                {
                    assetToLoadKeys.Add(assetKey);
                }
            }
            
            var assetWrappers = new List<AssetWrapper>(allKeys.Count);
            // 说明这个包的全部资源都加载过了，直接返回全部缓存即可
            if(assetToLoadKeys.Count == 0)
            {
                foreach (var cacheKey in allKeys)
                {
                    var assetWrapper = _assetWrappers[cacheKey];
                    assetWrapper.Retain();
                    assetWrappers.Add(assetWrapper);
                }
                return assetWrappers.ToArray();
            }
            
            // 说明这个包加载过资源，有缓存，加载剩余资源
            if (assetToLoadKeys.Count > 0 && assetToLoadKeys.Count != allKeys.Count)
            {
                var assetTasks = new List<Task<AssetWrapper>>();
                foreach (var assetKey in assetToLoadKeys)
                {
                    assetTasks.Add(LoadAssetAsync<T>(assetKey));
                }
                
                // 等待所有资源加载完成
                assetWrappers.AddRange(await Task.WhenAll(assetTasks));
                return assetWrappers.ToArray();
            }
            
            // 否则全量加载
            var bundleWrapper = await _assetBundleManager.LoadBundleAsync(bundleName);
            // 等待所有资源加载完成
            assetWrappers.AddRange(await bundleWrapper.LoadAllAssetAsync<T>());
            foreach (var assetWrapper in assetWrappers)
            {
                assetWrapper.Retain();
                _assetWrappers.TryAdd(assetWrapper.AssetKey, assetWrapper);
            }
            return assetWrappers.ToArray();
        }

        /// <summary>
        /// 获取所有场景路径，返回场景名列表
        /// </summary>
        /// <returns></returns>
        public List<string> GetAllScenePath()
        {
            var list = new List<string>();
            foreach (var entry in _assetBundleManager.Catalog.Assets)
            {
                if (entry.assetType == EAssetType.Scene)
                {
                    list.Add(entry.key);
                }
            }
            return list;
        }
        
        /// <summary>
        /// 释放资源，减少引用数
        /// </summary>
        /// <param name="key"></param>
        public void ReleaseWrapper(string key)
        {
            var assetWrapper = _assetWrappers.GetValueOrDefault(key);
            if(assetWrapper == null)
                return;
            
            assetWrapper.Release();
            if(assetWrapper.RefCount == 0)
            {
                _assetWrappers.Remove(key);
            }
        }
        
        /// <summary>
        /// 获取资源，图集中的图片使用GetSprite
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object GetAsset(string key)
        {
            var assetWrapper = _assetWrappers.GetValueOrDefault(key);
            return assetWrapper?.Asset;
        }

        /// <summary>
        /// 获取图集中图片资源
        /// </summary>
        /// <param name="atlasKey"></param>
        /// <param name="spriteKey"></param>
        /// <returns></returns>
        public Sprite GetSprite(string atlasKey, string spriteKey)
        {
            if (!_assetWrappers.TryGetValue(atlasKey, out var wrapper)) 
                return null;
            
            // 加载图片并缓存
            if (_sprites.TryGetValue((atlasKey, spriteKey), out var sprite)) 
                return sprite;
            
            sprite = ((SpriteAtlas)wrapper.Asset).GetSprite(spriteKey);
            _sprites.Add((atlasKey, spriteKey), sprite);
            return sprite;
        }
    }
}
