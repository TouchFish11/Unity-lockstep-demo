using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Tasks.Extensions;
using Core.Utility;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Core.AssetBundles.Management
{
    /// <summary>
    /// 游戏资源
    /// </summary>
    public static class GameAsset
    {
        /// <summary>
        /// 资源定位
        /// </summary>
        private class AssetLocation
        {
            /// <summary>
            /// 资源
            /// </summary>
            internal object Asset { get; set; } 
            
            /// <summary>
            /// 当前有效版本号
            /// </summary>
            internal int Version { get; set; }
            
            /// <summary>
            /// 资源引用计数
            /// </summary>
            internal int RefCount { get; set; }
            
            /// <summary>
            /// 资源释放回调
            /// </summary>
            internal Action release;
            
            // ... 其他元数据
        }
        
        private static IAssetBundleManager _assetBundleManager;
        // Key到句柄的映射
        private static readonly Dictionary<string, AssetHandle> _nameToAssetCacheMap = new();
        // 句柄ID到资源定位对象的映射
        private static readonly Dictionary<int, AssetLocation> _assetIdToLocationsMap = new();
        // 全局句柄ID
        private static int _nextId;
        // 句柄id缓存池
        private static readonly Queue<int> _idPool = new();
        
        public static void Init(IAssetBundleManager assetBundleManager)
        {
            _assetBundleManager = assetBundleManager;
        }
        
        public static async Task<AssetHandle<T>> LoadAssetAsync<T>(string key) where T : class
        {
            if (_nameToAssetCacheMap.TryGetValue(key, out var handle))
            {
                return handle.ConvertTo<T>();
            }
            
            // 从资源目录中查找指定的key
            var mapEntry = _assetBundleManager.Catalog.GetEntry(key);
            // 加载AB包
            var bundleWrapper = await _assetBundleManager.LoadBundleAsync(mapEntry.bundleName);
            // 异步加载资源
            var asset = await bundleWrapper.AssetBundle.LoadAssetAsync<T>(key).ToTask<T>();
            // 创建Handle
            var newHandle = new AssetHandle { HandleId = GenerateNewId(), Version = 0 };
            // 判断ID是否存在，存在就复用定位对象
            if (_assetIdToLocationsMap.TryGetValue(newHandle.HandleId, out var location))
            {
                location.Asset = asset;
                ++location.Version;
                location.RefCount = 1;
                location.release = () => bundleWrapper.Unload();
                // 缓存句柄
                _nameToAssetCacheMap.Add(key, newHandle);
                return newHandle.ConvertTo<T>();
            }
            
            // 创建定位对象
            var newLocation = new AssetLocation{Asset = asset, Version = 0, RefCount = 1, release = () => bundleWrapper.Unload()};
            // 缓存句柄
            _nameToAssetCacheMap.Add(key, newHandle);
            // 缓存定位对象
            _assetIdToLocationsMap.Add(newHandle.HandleId, newLocation);
            // 转换为泛型句柄
            return newHandle.ConvertTo<T>();
        }
        
        /// <summary>
        /// 异步加载多个资源
        /// </summary>
        /// <param name="keys">相同类型的多个资源键，传入不同类型的键其返回的资源为null</param>
        /// <typeparam name="T">资源类型</typeparam>
        /// <returns></returns>
        public static async Task<AssetHandle<IList<T>>> LoadAssetsAsync<T>(params string[] keys) where T : class
        {
            // 先对 keys 排序，确保同一组资源无论传入顺序如何都能命中同一缓存
            var sortedKeys = keys.OrderBy(k => k).ToArray();
            if (_nameToAssetCacheMap.TryGetValue(KeysToKey(sortedKeys), out var cacheHandle))
            {
                return cacheHandle.ConvertTo<IList<T>>();
            }
            
            var tasks = new List<Task<AssetHandle<T>>>();
            var handles = new List<AssetHandle>();
            foreach (var key in keys)
            {
                // 没有缓存就加载资源
                if (!_nameToAssetCacheMap.TryGetValue(key, out var handle))
                {
                    tasks.Add(LoadAssetAsync<T>(key));
                }
                // 获取缓存句柄
                else
                {
                    handles.Add(handle);
                }
            }
            
            // 等待所有资源加载完成
            var newHandles = await Task.WhenAll(tasks);
            foreach (var handle in newHandles) 
                handles.Add(handle);
            
            // 创建新Handle
            var newHandle = new AssetHandle { HandleId = GenerateNewId(), Version = 0 };
            IList<T> list = handles.ConvertAll(h => h.ConvertTo<T>().Asset);
            // 判断ID是否存在，存在就复用定位对象
            if (_assetIdToLocationsMap.TryGetValue(newHandle.HandleId, out var location))
            {
                location.Asset = list;
                ++location.Version;
                location.RefCount = 1;
                location.release = () => { foreach (var handle in handles) Release(handle); };
                // 缓存句柄
                _nameToAssetCacheMap.Add(KeysToKey(keys), newHandle);
                return newHandle.ConvertTo<IList<T>>();
            }
            
            // 创建新定位对象
            var newLocation = new AssetLocation
            {
                Asset = list, Version = newHandle.Version, RefCount = 1, release = () => { foreach (var handle in handles) Release(handle); }
            };

            // 缓存句柄
            _nameToAssetCacheMap.Add(KeysToKey(keys), newHandle);
            // 缓存定位对象
            _assetIdToLocationsMap.Add(newHandle.HandleId, newLocation);
            return newHandle.ConvertTo<IList<T>>();
        }

        // TODO：逻辑待定
        public static Task<List<string>> GetAllScenePathsAsync()
        {
            try
            {
                return Task.FromResult(new List<string>());
            }
            catch (Exception exception)
            {
                return Task.FromException<List<string>>(exception);
            }
        }

        // TODO：待完善
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
            if(!IsValidate(handle.HandleId,  handle.Version))
                return;
            
            var location = _assetIdToLocationsMap[handle.HandleId];
            // 减少引用计数
            --location.RefCount;
            if (location.RefCount > 0) 
                return;
            
            // 回收ID
            _idPool.Enqueue(handle.HandleId);
            location.release?.Invoke();
            location.release = null;
            // TODO：移除句柄缓存，可能需要反向缓存，反向查找
            //_nameToAssetCacheMap.Remove(handle.key?);
            // 不用清理location状态，下次复用ID会覆盖
        }

        internal static T GetAsset<T>(int handleId, int version) where T : class
        {
            if (!IsValidate(handleId, version)) 
                return null;
            
            var location = _assetIdToLocationsMap[handleId];
            ++location.RefCount;
            return location.Asset as T;
        }
        
        /// <summary>
        /// 生成句柄ID
        /// </summary>
        /// <returns></returns>
        private static int GenerateNewId()
        {
            while (_idPool.Count > 0)
            {
                return _idPool.Dequeue();
            }
            return ++_nextId;
        }

        /// <summary>
        /// 多个键拼接成一个Key缓存
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        private static string KeysToKey(params string[] keys)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < keys.Length; i++)
            {
                sb.Append(keys[i]);
                if(i <  keys.Length - 1)
                    sb.Append("_");
            }
            return sb.ToString();
        }
        
        /// <summary>
        /// 校验句柄有效性
        /// </summary>
        /// <param name="id"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        private static bool IsValidate(int id, int version)
        {
            return _assetIdToLocationsMap.TryGetValue(id, out var location) && location.Version == version;
        }
    }
}
