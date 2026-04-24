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
            if (_keyToHandleMap.TryGetValue(key, out var handle))
            {
                return !_assetIdToLocationsMap.ContainsKey(handle.HandleId) ?
                    throw new Exception($"{nameof(GameAsset)}:Resource management logic error") : handle.ConvertTo<T>();
            }

            var assetWrapper = _assetManager.LoadAsset<T>(key);
            AssetHandle newAssetHandle = CreateSingleHandle<T>(key);
            assetWrapper.OnUnload += () =>
            {
                // 回收ID
                _idPool.Enqueue(newAssetHandle.HandleId);
                // 移除缓存
                _keyToHandleMap.Remove(key);
                _handleToKeyMap.Remove(newAssetHandle);
            };

            return newAssetHandle.ConvertTo<T>();
        }

        // private static async Task<AssetHandle<T>> LoadSpriteAsync<T>(AssetEntry entry) where T : Object
        // {
        //     // 先获取图集
        //     SpriteAtlas spriteAtlas = null;
        //     BundleWrapper bundleWrapper = null;
        //     // 找到复用图集
        //     var spriteAssetEntry = entry as SpriteAssetEntry;
        //     if(_keyToHandleMap.TryGetValue(spriteAssetEntry.atlasKey, out var atlasHandle))
        //     {
        //         if (_assetIdToLocationsMap.TryGetValue(atlasHandle.HandleId, out var loc))
        //         {
        //             // 图集引用增加
        //             ++loc.RefCount;
        //             var handle = atlasHandle.ConvertTo<SpriteAtlas>();
        //             spriteAtlas = handle.Asset;
        //         }
        //     }
        //     // 加载图集
        //     else
        //     {
        //         // 加载图集AB包
        //         bundleWrapper = await _assetBundleManager.LoadBundleAsync(spriteAssetEntry.bundleName);
        //         // 异步加载图集资源
        //         var assetWrapper = await bundleWrapper.LoadAssetAsync<T>(spriteAssetEntry.spriteAssetName);
        //         if (assetWrapper.IsNull)
        //             throw new NullReferenceException($"{nameof(GameAsset)}: load {spriteAssetEntry.spriteAssetName} failed, key({spriteAssetEntry.key})");
        //         
        //         // 避免逻辑上重复添加
        //         if (_keyToHandleMap.TryGetValue(spriteAssetEntry.atlasKey, out var assetHandle))
        //         {
        //             if (!_assetIdToLocationsMap.TryGetValue(assetHandle.HandleId, out var loc))
        //                 throw new Exception($"{nameof(GameAsset)}:Resource management logic error");
        //         
        //             ++loc.RefCount;
        //             spriteAtlas = assetHandle.ConvertTo<SpriteAtlas>().Asset;
        //         }
        //         else
        //         {
        //             // 创建新图集Handle
        //             var newAtlasHandle = new AssetHandle { HandleId = GenerateNewId(), Version = 0 };
        //             // 判断ID是否存在，存在就复用定位对象
        //             if (_assetIdToLocationsMap.TryGetValue(newAtlasHandle.HandleId, out var atlaslocation))
        //             {
        //                 atlaslocation.AssetWrapper = assetWrapper;
        //                 ++atlaslocation.Version;
        //                 atlaslocation.RefCount = 1;
        //                 atlaslocation.release = bundleWrapper.Release;
        //                 // 同步新句柄的版本
        //                 newAtlasHandle.Version = atlaslocation.Version;
        //                 // 缓存新句柄
        //                 _keyToHandleMap.Add(spriteAssetEntry.atlasKey, newAtlasHandle);
        //                 _handleToKeyMap.Add(newAtlasHandle, spriteAssetEntry.atlasKey);
        //                 // 获取图集资源
        //                 spriteAtlas = newAtlasHandle.ConvertTo<SpriteAtlas>().Asset;
        //             }
        //             else
        //             {
        //                 // 创建新定位对象
        //                 var newLocation = new AssetLocation
        //                 {
        //                     AssetWrapper = assetWrapper, Version = 0, RefCount = 1, release = bundleWrapper.Release
        //                 };
        //                 // 缓存新句柄
        //                 _keyToHandleMap.TryAdd(spriteAssetEntry.atlasKey, newAtlasHandle);
        //                 _handleToKeyMap.TryAdd(newAtlasHandle, spriteAssetEntry.atlasKey);
        //                 // 缓存新定位对象
        //                 _assetIdToLocationsMap.TryAdd(newAtlasHandle.HandleId, newLocation);
        //                 // 获取图集资源
        //                 spriteAtlas = newAtlasHandle.ConvertTo<SpriteAtlas>().Asset;
        //             }
        //         }
        //     }
        //
        //     // 这里可以不用判断，外部已经判断过了，图片资源被缓存过，直接返回句柄
        //     if (_keyToHandleMap.TryGetValue(entry.key, out var spriteHandle))
        //     {
        //         if (!_assetIdToLocationsMap.TryGetValue(spriteHandle.HandleId, out var loc))
        //             throw new Exception($"{nameof(GameAsset)}:Resource management logic error");
        //         
        //         ++loc.RefCount;
        //         return spriteHandle.ConvertTo<T>();
        //     }
        //     
        //     // 从图集中加载新图片资源
        //     var sprite = spriteAtlas.GetSprite(spriteAssetEntry.key);
        //     var spriteAssetWrapper = DIContainer.Create<AssetWrapper>(parameterValues: new object[] { sprite, bundleWrapper });
        //     // 创建新图片句柄
        //     var newHandle = new AssetHandle { HandleId = GenerateNewId(), Version = 0 };
        //     // 判断ID是否存在，存在就复用定位对象
        //     if (_assetIdToLocationsMap.TryGetValue(newHandle.HandleId, out var spriteLocation))
        //     {
        //         // 图片资源包装，资源是图片，包是图集包
        //         spriteLocation.AssetWrapper = spriteAssetWrapper;
        //         ++spriteLocation.Version;
        //         spriteLocation.RefCount = 1;
        //         spriteLocation.release = bundleWrapper.Release;
        //         // 同步新句柄的版本
        //         newHandle.Version = spriteLocation.Version;
        //         // 缓存新句柄
        //         _keyToHandleMap.Add(spriteAssetEntry.key, newHandle);
        //         _handleToKeyMap.Add(newHandle, spriteAssetEntry.key);
        //         return newHandle.ConvertTo<T>();
        //     }
        //     
        //     // 创建新定位对象
        //     var newSpriteLocation = new AssetLocation
        //     {
        //         AssetWrapper = spriteAssetWrapper, Version = 0, RefCount = 1, release = bundleWrapper.Release
        //     };
        //     // 缓存新句柄
        //     _keyToHandleMap.TryAdd(spriteAssetEntry.key, newHandle);
        //     _handleToKeyMap.TryAdd(newHandle, spriteAssetEntry.key);
        //     // 缓存新定位对象
        //     _assetIdToLocationsMap.TryAdd(newHandle.HandleId, newSpriteLocation);
        //     // 获取图集资源
        //     return newHandle.ConvertTo<T>();
        // }
        
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
            // 存在资源缓存句柄，直接返回
            if (_keyToHandleMap.TryGetValue(key, out var handle))
            {
                return !_assetIdToLocationsMap.ContainsKey(handle.HandleId) ? 
                    throw new Exception($"{nameof(GameAsset)}:Resource management logic error") : handle.ConvertTo<T>();
            }

            // 异步加载资源
            var assetWrapper = await _assetManager.LoadAssetAsync<T>(key);
            // 避免并发逻辑重复添加
            if (_keyToHandleMap.TryGetValue(key, out var assetHandle))
            {
                return !_assetIdToLocationsMap.ContainsKey(assetHandle.HandleId) ? 
                    throw new Exception($"{nameof(GameAsset)}:Resource management logic error") : assetHandle.ConvertTo<T>();
            }
            
            AssetHandle newAssetHandle = CreateSingleHandle<T>(key);
            assetWrapper.OnUnload += () =>
            {
                // 回收ID
                _idPool.Enqueue(newAssetHandle.HandleId);
                // 移除缓存
                _keyToHandleMap.Remove(key);
                _handleToKeyMap.Remove(newAssetHandle);
            };
            
            return newAssetHandle.ConvertTo<T>();
            
            // // 创建新资源Handle
            // var newAssetHandle = new AssetHandle { HandleId = GenerateNewId(), Version = 0 };
            // // 当资源被卸载的时候，回收句柄ID，移除句柄缓存
            // assetWrapper.OnUnload += () =>
            // {
            //     // 回收ID
            //     _idPool.Enqueue(newAssetHandle.HandleId);
            //     // 移除缓存
            //     _keyToHandleMap.Remove(key);
            //     _handleToKeyMap.Remove(newAssetHandle);
            // };
            //
            // // 判断ID是否存在，存在就复用定位对象
            // if (_assetIdToLocationsMap.TryGetValue(newAssetHandle.HandleId, out var location))
            // {
            //     location.AssetKey = key;
            //     ++location.Version;
            //     // 同步新句柄的版本
            //     newAssetHandle.Version = location.Version;
            //     // 缓存新句柄
            //     _keyToHandleMap.Add(key, newAssetHandle);
            //     _handleToKeyMap.Add(newAssetHandle, key);
            //     return newAssetHandle.ConvertTo<T>();
            // }
            //
            // // 创建新定位对象
            // var newLocation = new AssetLocation { AssetKey = key, Version = 0 };
            // // 缓存新句柄
            // _keyToHandleMap.Add(key, newAssetHandle);
            // _handleToKeyMap.Add(newAssetHandle, key);
            // // 缓存新定位对象
            // _assetIdToLocationsMap.Add(newAssetHandle.HandleId, newLocation);
            // // 转换为泛型句柄
            // return newAssetHandle.ConvertTo<T>();
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
            
            if (_keyToHandleMap.TryGetValue(combinedKey, out var cacheHandle))
            {
                return !_assetIdToLocationsMap.ContainsKey(cacheHandle.HandleId) ? 
                    throw new Exception($"{nameof(GameAsset)}:Resource management logic error") : cacheHandle.ConvertTo<T>();
            }
            
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

            #region MyRegion

            // // 创建新Handle，并且是组合句柄，该句柄对应的定位对象不直接指向资源，该句柄存储所有持有资源的子句柄
            // var newHandle = new AssetHandle
            // {
            //     HandleId = GenerateNewId(), Version = 0, IsCombine = true,
            //     CombineHandles = new List<AssetHandle>(allHandles)
            // };
            //
            // // 判断ID是否存在，存在就复用定位对象
            // if (_assetIdToLocationsMap.TryGetValue(newHandle.HandleId, out var location))
            // {
            //     location.AssetKey = combinedKey;
            //     ++location.Version;
            //     // 同步定位对象的版本到句柄
            //     newHandle.Version = location.Version;
            //     // 缓存句柄
            //     _keyToHandleMap.Add(combinedKey, newHandle);
            //     _handleToKeyMap.Add(newHandle, combinedKey);
            //     return newHandle.ConvertTo<IList<T>>();
            // }
            //
            // // 创建新定位对象
            // var newLocation = new AssetLocation { AssetKey = combinedKey, Version = newHandle.Version };
            // // 缓存句柄
            // _keyToHandleMap.Add(combinedKey, newHandle);
            // _handleToKeyMap.Add(newHandle, combinedKey);
            // // 缓存定位对象
            // _assetIdToLocationsMap.Add(newHandle.HandleId, newLocation);
            // return newHandle.ConvertTo<IList<T>>();

            #endregion
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
            if (_keyToHandleMap.TryGetValue(bundleKey, out var cacheHandle))
            {
                return !_assetIdToLocationsMap.ContainsKey(cacheHandle.HandleId) ? 
                    throw new Exception($"{nameof(GameAsset)}:Resource management logic error") : cacheHandle.ConvertTo<T>();
            }
            
            // 异步加载包所有资源
            var assetWrappers = await _assetManager.LoadAllAssetAsync<T>(bundleName);
            
            // 避免逻辑上重复添加
            if (_keyToHandleMap.TryGetValue(bundleKey, out var handle))
            {
                return !_assetIdToLocationsMap.ContainsKey(handle.HandleId) ? 
                    throw new Exception($"{nameof(GameAsset)}:Resource management logic error") : handle.ConvertTo<T>();
            }
            
            var allHandles = new List<AssetHandle>();
            // 遍历所有资源包装，创建每个资源的句柄
            foreach (var assetWrapper in assetWrappers)
            {
                var assetHandle = CreateSingleHandle<T>(assetWrapper.AssetKey);
                allHandles.Add(assetHandle);
                
                #region Old
                // // 创建新Handle
                // var newHandle = new AssetHandle { HandleId = GenerateNewId(), Version = 0 };
                // // 判断ID是否存在，存在就复用定位对象
                // if (_assetIdToLocationsMap.TryGetValue(newHandle.HandleId, out var location))
                // {
                //     location.AssetKey = assetWrapper.AssetKey;
                //     ++location.Version;
                //     // 同步定位对象的版本
                //     newHandle.Version = location.Version;
                //     // 缓存句柄，通过AB包名作为句柄的Key
                //     _keyToHandleMap.Add(bundleKey, newHandle);
                //     _handleToKeyMap.Add(newHandle, bundleKey);
                //     return newHandle.ConvertTo<IList<T>>();
                // }
                //
                // // 创建新定位对象
                // var newLocation = new AssetLocation { AssetKey = assetWrapper.AssetKey, Version = newHandle.Version };
                // // 缓存句柄
                // _keyToHandleMap.Add(bundleKey, newHandle);
                // _handleToKeyMap.Add(newHandle, bundleKey);
                // // 缓存定位对象
                // _assetIdToLocationsMap.Add(newHandle.HandleId, newLocation);
                // return newHandle.ConvertTo<IList<T>>();
                #endregion
            }
            
            // 返回组合句柄
            return CreateCombineHandle<T>(bundleKey, allHandles);

            #region MyRegion

            // 创建新Handle，并且是组合句柄，该句柄对应的定位对象不直接指向资源，该句柄存储所有持有资源的子句柄
            // var newHandle = new AssetHandle
            // {
            //     HandleId = GenerateNewId(), Version = 0, IsCombine = true,
            //     CombineHandles = new List<AssetHandle>(allHandles)
            // };
            //
            // // 判断ID是否存在，存在就复用定位对象
            // if (_assetIdToLocationsMap.TryGetValue(newHandle.HandleId, out var location))
            // {
            //     location.AssetKey = bundleKey;
            //     ++location.Version;
            //     // 同步定位对象的版本到句柄
            //     newHandle.Version = location.Version;
            //     // 缓存句柄
            //     _keyToHandleMap.Add(bundleKey, newHandle);
            //     _handleToKeyMap.Add(newHandle, bundleKey);
            //     return newHandle.ConvertTo<T>();
            // }
            //
            // // 创建新定位对象
            // var newLocation = new AssetLocation { AssetKey = bundleKey, Version = newHandle.Version };
            // // 缓存句柄
            // _keyToHandleMap.Add(bundleKey, newHandle);
            // _handleToKeyMap.Add(newHandle, bundleKey);
            // // 缓存定位对象
            // _assetIdToLocationsMap.Add(newHandle.HandleId, newLocation);
            // return newHandle.ConvertTo<T>();

            #endregion
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
                // 缓存句柄，通过AB包名作为句柄的Key
                _keyToHandleMap.Add(key, newHandle);
                _handleToKeyMap.Add(newHandle, key);
                return newHandle.ConvertTo<T>();
            }
            
            // 工厂创建新定位对象
            var newLocation = AssetLocationFactory.GetAssetLocation<T>(_assetManager.GetAssetEntry(key), newHandle.Version);
            // 缓存句柄
            _keyToHandleMap.Add(key, newHandle);
            _handleToKeyMap.Add(newHandle, key);
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
                // 缓存句柄
                _keyToHandleMap.Add(combineKey, newHandle);
                _handleToKeyMap.Add(newHandle, combineKey);
                return newHandle.ConvertTo<T>();
            }
            
            // 工厂创建新定位对象
            var newLocation = AssetLocationFactory.GetAssetLocationCombine(combineKey, newHandle.Version);
            // 缓存句柄
            _keyToHandleMap.Add(combineKey, newHandle);
            _handleToKeyMap.Add(newHandle, combineKey);
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
                _keyToHandleMap.Remove(_handleToKeyMap.GetValueOrDefault(handle));
                _handleToKeyMap.Remove(handle);
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
