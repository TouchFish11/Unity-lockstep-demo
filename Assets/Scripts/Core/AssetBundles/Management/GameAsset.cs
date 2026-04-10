using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Tasks.Extensions;
using UnityEngine;

namespace Core.AssetBundles.Management
{
    public static class GameAsset
    {
        private static IAssetBundleManager _assetBundleManager;
        private static readonly Dictionary<string, AssetHandle> _nameToAssetCacheMap = new();
        
        public static void Init(IAssetBundleManager assetBundleManager)
        {
            _assetBundleManager = assetBundleManager;
        }
        
        public static async Task<AssetHandle<T>> LoadAssetAsync<T>(string key) where T : Object
        {
            if (_nameToAssetCacheMap.TryGetValue(key, out var handle))
            {
                handle.Retain();
                return handle.ConvertTo<AssetHandle<T>>();
            }
            
            // 查表key
            var mapEntry = _assetBundleManager.Catalog.GetEntry(key);
            // 加载AB
            var bundleWrapper = await _assetBundleManager.LoadBundleAsync(mapEntry.bundleName);
            // LoadAsset
            var asset = await bundleWrapper.AssetBundle.LoadAssetAsync<T>(key).ToTask<T>();
            // 创建Handle
            var assetHandle = new AssetHandle<T>{Asset = asset, release = () => bundleWrapper.Unload()};
            // 缓存句柄
            _nameToAssetCacheMap.Add(key, assetHandle);
            return assetHandle;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="keys"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async Task<BatchHandle<T>> LoadAssetsAsync<T>(params string[] keys) where T : Object
        {
            var tasks = new List<Task<AssetHandle<T>>>();
            var handles = new List<AssetHandle<T>>();
            foreach (var key in keys)
            {
                if (!_nameToAssetCacheMap.TryGetValue(key, out var handle))
                {
                    tasks.Add(LoadAssetAsync<T>(key));
                }
                else
                {
                    handle.Retain();
                    handles.Add(handle as AssetHandle<T>);
                }
            }
            
            var newHandles = await Task.WhenAll(tasks);
            handles.AddRange(newHandles);
            var batchHandle = new BatchHandle<T>(handles);
            return batchHandle;
        }

        public static async Task<List<string>> GetAllScenePathsAsync()
        {
            return new List<string>();
        }

        public static async Task<GameObject> InstanceAsync(GameObject prefabAsset)
        {
            var tcs = new TaskCompletionSource<bool>();
            var operation = Object.InstantiateAsync(prefabAsset);
            operation.completed += _ => tcs.SetResult(true);
            await tcs.Task;
            return operation.Result[0];
        }

        /// <summary>
        /// 释放句柄
        /// </summary>
        /// <param name="handle"></param>
        public static void Release(AssetHandle handle)
        {
            handle.Release();
        }
    }
}
