using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.DI;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Core.AssetBundles.Management
{
    /// <summary>
    /// 游戏资源
    /// </summary>
    public static class GameAsset
    {
        // 资源管理器
        private static AssetManager _assetManager;
        // Key到句柄的映射
        //private static readonly Dictionary<string, AssetHandle> _keyToHandleMap = new();
        // 句柄到Key的映射
        //private static readonly Dictionary<AssetHandle, string> _handleToKeyMap = new();
        // 句柄ID到资源定位对象的映射
        private static readonly Dictionary<int, AssetLocation> _assetIdToLocationsMap = new();
        // 全局句柄ID
        private static int _nextId;
        // 句柄id缓存池
        private static readonly Queue<int> _idPool = new();
        
        internal static void Init(IAssetBundleManager assetBundleManager)
        {
            _assetManager = DIContainer.Create<AssetManager>(parameterValues: assetBundleManager);
        }

        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <param name="key"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static AssetHandle<T> LoadAsset<T>(string key) where T : Object
        {
            var assetWrapper = _assetManager.LoadAsset<T>(key);
            AssetHandle newAssetHandle = CreateSingleHandle<T>(key);
            assetWrapper.OnUnload += () =>
            {
                // 回收ID
                _idPool.Enqueue(newAssetHandle.HandleId);
            };

            return newAssetHandle.ConvertTo<T>();
        }
        
        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="key"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        /// <exception cref="NullReferenceException"></exception>
        public static async Task<AssetHandle<T>> LoadAssetAsync<T>(string key) where T : Object
        {
            // 异步加载资源
            var assetWrapper = await _assetManager.LoadAssetAsync<T>(key);
            // 创建新句柄
            AssetHandle newAssetHandle = CreateSingleHandle<T>(key);
            // 资源包装不为空才去监听事件，否则直接返回句柄，外部通过句柄获取的资源就是null
            if (assetWrapper != null)
            {
                assetWrapper.OnUnload += () =>
                {
                    // 回收句柄ID
                    _idPool.Enqueue(newAssetHandle.HandleId);
                };
            }
            return newAssetHandle.ConvertTo<T>();
        }
        
        /// <summary>
        /// 异步加载相同类型的多个资源
        /// </summary>
        /// <param name="keys">相同类型的多个资源键，传入不同类型的键其返回的资源为null</param>
        /// <typeparam name="T">资源类型</typeparam>
        /// <returns></returns>
        public static async Task<AssetHandle<T>> LoadAssetsAsync<T>(params string[] keys) where T : Object
        {
            // 先对 keys 排序，确保同一组资源无论传入顺序如何都能命中同一缓存
            var sortedKeys = keys.OrderBy(k => k).ToArray();
            var combinedKey = KeysToKey(sortedKeys);
            
            var tasks = new List<Task<AssetHandle<T>>>();
            var allHandles = new List<AssetHandle>();
            foreach (var key in keys)
            {
                tasks.Add(LoadAssetAsync<T>(key));
            }
            
            // 等待所有资源加载完成
            var newHandles = await Task.WhenAll(tasks);
            foreach (var handle in newHandles) 
                allHandles.Add(handle);
            
            // 返回组合句柄
            return CreateCombineHandle<T>(combinedKey, allHandles);
        }

        /// <summary>
        /// 异步加载指定AB包中的所有资源
        /// </summary>
        /// <param name="bundleName">AB包名</param>
        /// <typeparam name="T">资源类型</typeparam>
        /// <returns></returns>
        public static async Task<AssetHandle<T>> LoadAllAssetAsync<T>(string bundleName) where T : Object
        {
            // 包名加类型名作为Key
            var bundleKey = $"{bundleName}_{typeof(T)}";
            // 异步加载包所有资源
            var assetWrappers = await _assetManager.LoadAllAssetAsync<T>(bundleName);

            var allHandles = new List<AssetHandle>();
            // 遍历所有资源包装，创建每个资源的句柄
            foreach (var assetWrapper in assetWrappers)
            {
                var assetHandle = CreateSingleHandle<T>(assetWrapper.AssetKey);
                allHandles.Add(assetHandle);
            }
            
            // 返回组合句柄
            return CreateCombineHandle<T>(bundleKey, allHandles);
        }

        /// <summary>
        /// 创建简单句柄，非组合句柄对象
        /// </summary>
        /// <param name="key"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private static AssetHandle<T> CreateSingleHandle<T>(string key) where T : class
        {
            // 创建新Handle
            var newHandle = new AssetHandle { HandleId = GenerateNewId(), Version = 0 };
            // 判断ID是否存在，存在就复用定位对象
            if (_assetIdToLocationsMap.TryGetValue(newHandle.HandleId, out var location))
            {
                location.AssetKey = key;
                ++location.Version;
                // 同步定位对象的版本
                newHandle.Version = location.Version;
                return newHandle.ConvertTo<T>();
            }
            
            // 工厂创建新定位对象
            var newLocation = AssetLocationFactory.GetAssetLocation<T>(_assetManager.GetAssetEntry(key), newHandle.Version);
            // 缓存定位对象
            _assetIdToLocationsMap.Add(newHandle.HandleId, newLocation);
            return newHandle.ConvertTo<T>();
        }

        private static AssetHandle<T> CreateCombineHandle<T>(string combineKey, IEnumerable<AssetHandle> subHandles) where T : class
        {
            // 创建新Handle，并且是组合句柄，该句柄对应的定位对象不直接指向资源，该句柄存储所有持有资源的子句柄
            var newHandle = new AssetHandle
            {
                HandleId = GenerateNewId(), Version = 0, IsCombine = true,
                CombineHandles = new List<AssetHandle>(subHandles)
            };
            
            // 判断ID是否存在，存在就复用定位对象
            if (_assetIdToLocationsMap.TryGetValue(newHandle.HandleId, out var location))
            {
                location.AssetKey = combineKey;
                ++location.Version;
                // 同步定位对象的版本到句柄
                newHandle.Version = location.Version;
                return newHandle.ConvertTo<T>();
            }
            
            // 工厂创建新定位对象
            var newLocation = AssetLocationFactory.GetAssetLocationCombine(combineKey, newHandle.Version);
            // 缓存定位对象
            _assetIdToLocationsMap.Add(newHandle.HandleId, newLocation);
            return newHandle.ConvertTo<T>();
        }
        
        /// <summary>
        /// 获取所有的场景路径（key）
        /// </summary>
        /// <returns></returns>
        public static List<string> GetAllScenePath()
        {
            return _assetManager.GetAllScenePath();
        }

        /// <summary>
        /// 释放句柄
        /// </summary>
        /// <param name="handle"></param>
        public static void Release(AssetHandle handle)
        {
            if(!IsValidate(handle.HandleId,  handle.Version))
                return;

            if (handle.IsCombine)
            {
                foreach (var assetHandle in handle.CombineHandles)
                {
                    Release(assetHandle);
                }
                
                // 需要主动移除该句柄本身，因为组合的句柄本身不直接指向某个资源，所以不会响应资源卸载的回调
                _idPool.Enqueue(handle.HandleId);
            }
            else
            {
                var location = _assetIdToLocationsMap.GetValueOrDefault(handle.HandleId);
                if (location == null)
                    return;
                // 减少引用计数
                _assetManager.ReleaseWrapper(location.AssetKey);
            }
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
            
            var location = _assetIdToLocationsMap.GetValueOrDefault(handleId);
            if(location == null)
                return null;
            
            if (location is spriteLocation spriteLocation)
            {
                return _assetManager.GetSprite(spriteLocation.AssetKey, spriteLocation.SpriteKey) as T;
            }

            var asset = _assetManager.GetAsset(location.AssetKey);
            // 资源本身是GameObject，T要是组件类型，从资源中获取对应组件类型返回
            if (asset is GameObject objAsset && typeof(Component).IsAssignableFrom(typeof(T)))
                return objAsset.GetComponent<T>();
            // 否则直接转换为T返回，比如非实例化资源，List，纯GameObject
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
