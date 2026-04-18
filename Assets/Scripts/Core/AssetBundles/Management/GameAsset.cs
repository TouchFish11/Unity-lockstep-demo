using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Tasks.Extensions;
using UnityEngine;

namespace Core.AssetBundles.Management
{
    /// <summary>
    /// 游戏资源
    /// </summary>
    public static class GameAsset
    {
        private static IAssetBundleManager _assetBundleManager;
        // Key到句柄的映射
        private static readonly Dictionary<string, AssetHandle> _keyToHandleMap = new();
        // 句柄到Key的映射
        private static readonly Dictionary<AssetHandle, string> _handleToKeyMap = new();
        // 句柄ID到资源定位对象的映射
        private static readonly Dictionary<int, AssetLocation> _assetIdToLocationsMap = new();
        // 全局句柄ID
        private static int _nextId;
        // 句柄id缓存池
        private static readonly Queue<int> _idPool = new();
        
        internal static void Init(IAssetBundleManager assetBundleManager)
        {
            _assetBundleManager = assetBundleManager;
        }
        
        public static async Task<AssetHandle<T>> LoadAssetAsync<T>(string key) where T : class
        {
            if (_keyToHandleMap.TryGetValue(key, out var handle))
            {
                if (!_assetIdToLocationsMap.TryGetValue(handle.HandleId, out var loc))
                    throw new Exception($"{nameof(GameAsset)}:Resource management logic error");
                
                ++loc.RefCount;
                return handle.ConvertTo<T>();
            }
            
            // 从资源目录中查找指定的资源路径
            var mapEntry = _assetBundleManager.Catalog.GetEntry(key);
            if (mapEntry == null)
                throw new NullReferenceException($"{nameof(GameAsset)}: key({key}) found entry is null");
                
            // 加载AB包
            var bundleWrapper = await _assetBundleManager.LoadBundleAsync(mapEntry.bundleName);
            if (bundleWrapper == null)
                throw new NullReferenceException($"{nameof(GameAsset)}: load {mapEntry.bundleName} AssetBundle failed");
            
            // 异步加载资源
            var asset = await bundleWrapper.AssetBundle.LoadAssetAsync<T>(mapEntry.assetName).ToTask<T>();
            if (asset == null)
                throw new NullReferenceException($"{nameof(GameAsset)}: load {mapEntry.assetName} failed");
            
            // 创建Handle
            var newHandle = new AssetHandle { HandleId = GenerateNewId(), Version = 0 };
            // 判断ID是否存在，存在就复用定位对象
            if (_assetIdToLocationsMap.TryGetValue(newHandle.HandleId, out var location))
            {
                location.Asset = asset;
                ++location.Version;
                location.RefCount = 1;
                location.release = () => bundleWrapper.Release();
                // 同步定位对象的版本
                newHandle.Version = location.Version;
                // 缓存句柄
                _keyToHandleMap.Add(key, newHandle);
                _handleToKeyMap.Add(newHandle, key);
                return newHandle.ConvertTo<T>();
            }
            
            // 创建定位对象
            var newLocation = new AssetLocation{Asset = asset, Version = 0, RefCount = 1, release = () => bundleWrapper.Release()};
            // 缓存句柄
            _keyToHandleMap.Add(key, newHandle);
            _handleToKeyMap.Add(newHandle, key);
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
            var combinedKey = KeysToKey(sortedKeys);
            if (_keyToHandleMap.TryGetValue(combinedKey, out var cacheHandle))
            {
                if (!_assetIdToLocationsMap.TryGetValue(cacheHandle.HandleId, out var loc))
                    throw new Exception($"{nameof(GameAsset)}:Resource management logic error");
                
                ++loc.RefCount;
                return cacheHandle.ConvertTo<IList<T>>();
            }
            
            var tasks = new List<Task<AssetHandle<T>>>();
            var handles = new List<AssetHandle>();
            foreach (var key in keys)
            {
                // 没有缓存就加载资源
                if (!_keyToHandleMap.TryGetValue(key, out var handle))
                {
                    tasks.Add(LoadAssetAsync<T>(key));
                }
                // 复用缓存句柄
                else
                {
                    ++_assetIdToLocationsMap[handle.HandleId].RefCount;
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
                // 同步定位对象的版本
                newHandle.Version = location.Version;
                // 缓存句柄
                _keyToHandleMap.Add(combinedKey, newHandle);
                _handleToKeyMap.Add(newHandle, combinedKey);
                return newHandle.ConvertTo<IList<T>>();
            }
            
            // 创建新定位对象
            var newLocation = new AssetLocation
            {
                Asset = list, Version = newHandle.Version, RefCount = 1, release = () => { foreach (var handle in handles) Release(handle); }
            };

            // 缓存句柄
            _keyToHandleMap.Add(combinedKey, newHandle);
            _handleToKeyMap.Add(newHandle, combinedKey);
            // 缓存定位对象
            _assetIdToLocationsMap.Add(newHandle.HandleId, newLocation);
            return newHandle.ConvertTo<IList<T>>();
        }

        /// <summary>
        /// 加载指定AB包中的所有资源
        /// </summary>
        /// <param name="bundleName">AB包名</param>
        /// <typeparam name="T">资源类型</typeparam>
        /// <returns></returns>
        public static async Task<AssetHandle<IList<T>>> LoadAllAssetByBundleAsync<T>(string bundleName) where T : class
        {
            var tasks = new List<Task<AssetHandle<T>>>();
            var handles = new List<AssetHandle>();
            var keys = _assetBundleManager.Catalog.GetAssetKeysByBundle(bundleName);
            foreach (var key in keys)
            {
                // 没有缓存就加载资源
                if (!_keyToHandleMap.TryGetValue(key, out var handle))
                {
                    tasks.Add(LoadAssetAsync<T>(key));
                }
                // 复用缓存句柄
                else
                {
                    ++_assetIdToLocationsMap[handle.HandleId].RefCount;
                    handles.Add(handle);
                }
            }
            
            // 等待所有资源加载完成
            var newHandles = await Task.WhenAll(tasks);
            foreach (var handle in newHandles) 
                handles.Add(handle);

            var bundleKey = $"{bundleName}_{typeof(T)}";
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
                // 同步定位对象的版本
                newHandle.Version = location.Version;
                // 缓存句柄，通过AB包名作为句柄的Key
                _keyToHandleMap.Add(bundleKey, newHandle);
                _handleToKeyMap.Add(newHandle, bundleKey);
                return newHandle.ConvertTo<IList<T>>();
            }
            
            // 创建新定位对象
            var newLocation = new AssetLocation
            {
                Asset = list, Version = newHandle.Version, RefCount = 1, release = () => { foreach (var handle in handles) Release(handle); }
            };

            // 缓存句柄
            _keyToHandleMap.Add(bundleKey, newHandle);
            _handleToKeyMap.Add(newHandle, bundleKey);
            // 缓存定位对象
            _assetIdToLocationsMap.Add(newHandle.HandleId, newLocation);
            return newHandle.ConvertTo<IList<T>>();
        }
        
        /// <summary>
        /// 获取所有的场景路径（key）
        /// </summary>
        /// <returns></returns>
        public static List<string> GetAllScenePath()
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
            // 执行释放回调
            location.release?.Invoke();
            location.release = null;
            // 移除句柄缓存，反向查找
            var key = _handleToKeyMap.GetValueOrDefault(handle);
            _keyToHandleMap.Remove(key);
            _handleToKeyMap.Remove(handle);
            // 不用清理location状态，下次复用ID会覆盖
        }

        /// <summary>
        /// 获取类型资源，若验证的id和版本无效，返回null；泛型句柄提供资源给外部时使用该方法返回对应的资源
        /// </summary>
        /// <param name="handleId">句柄唯一ID</param>
        /// <param name="version">版本号</param>
        /// <typeparam name="T">资源类型</typeparam>
        /// <returns>若该句柄对应的资源是GameObject，则T返回该资源身上的组件；非GameObject直接返回该资源</returns>
        internal static T GetAsset<T>(int handleId, int version) where T : class
        {
            if (!IsValidate(handleId, version)) 
                return null;

            var asset = _assetIdToLocationsMap[handleId].Asset;
            // 资源本身是GameObject，但T要是组件类型
            if (asset is GameObject objAsset && typeof(Component).IsAssignableFrom(typeof(T)))
                return objAsset.GetComponent<T>();
            return asset as T;
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
