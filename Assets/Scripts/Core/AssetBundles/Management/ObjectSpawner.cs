using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.DI;
using Core.Mono;
using Core.Pool;
using Unity.VisualScripting;
using UnityEngine;
using Logger = Core.Log.Logger;
using Object = UnityEngine.Object;

namespace Core.AssetBundles.Management
{
    /// <summary>
    /// 对象生成器
    /// </summary>
    public class ObjectSpawner : IDisposable, IPoolData
    {
        [Inject] private IPoolManager _poolManager;
        // 资源Key到资源句柄的映射
        private readonly Dictionary<string, AssetHandle> _keyToHandleMap = new();
        // 资源Key到资源加载任务的映射
        private readonly Dictionary<string, Task<AssetHandle<GameObject>>> _keyToHandleTaskMap = new();
        // 缓存加载过的资源Key
        private readonly HashSet<string> _assetKeys = new();

        /// <summary>
        /// 生成对象
        /// </summary>
        /// <param name="key"></param>
        /// <param name="parent"></param>
        /// <param name="pos"></param>
        /// <param name="rot"></param>
        /// <param name="worldSpace"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public PoolObject<T> Spawn<T>(string key, Transform parent = null, Vector3 pos = default, Quaternion rot = default, bool worldSpace = false) where T : Object
        {
            // 尝试从对象池获取
            var poolObject = GetPoolObject<T>(key, parent, pos, rot, worldSpace);
            if(poolObject.Obj)
                return poolObject.Convert<T>();

            // 复用句柄资源实例化
            if (_keyToHandleMap.TryGetValue(key, out var handle))
            {
                return Instantiate<T>(handle, key, parent, pos, rot, worldSpace);
            }
            
            // 加载资源
            handle = GameAsset.LoadAsset<GameObject>(key);
            // 缓存Key
            _assetKeys.Add(key);
            // 缓存句柄
            _keyToHandleMap.Add(key, handle);
            Logger.Log($"[{nameof(ObjectSpawner)}]: Cache asset({key}), Cache handle id({handle.HandleId})");
            // 实例化
            return Instantiate<T>(handle, key, parent, pos, rot, worldSpace);
        }
        
        /// <summary>
        /// 异步生成对象
        /// </summary>
        /// <param name="key"></param>
        /// <param name="parent"></param>
        /// <param name="pos"></param>
        /// <param name="rot"></param>
        /// <param name="worldSpace"></param>
        /// <typeparam name="T">游戏对象上的组件类型</typeparam>
        /// <returns>返回该游戏对象上的特定组件</returns>
        public async Task<PoolObject<T>> SpawnAsync<T>(string key, Transform parent = null, Vector3 pos = default, Quaternion rot = default, bool worldSpace = false) where T : Object
        {
            // 尝试从对象池获取
            var poolObject = GetPoolObject<T>(key, parent, pos, rot, worldSpace);
            if(poolObject.Obj)
                return poolObject.Convert<T>();

            // 先从缓存句柄中获取
            if (_keyToHandleMap.TryGetValue(key, out var assetHandle))
            {
                // 实例化资源
                return Instantiate<T>(assetHandle, key, parent, pos, rot, worldSpace);
            }
            
            // 返回正在加载的任务
            if (_keyToHandleTaskMap.TryGetValue(key, out var cacheTask))
            {
                var handle = await cacheTask;
                return Instantiate<T>(handle, key, parent, pos, rot, worldSpace);
            }

            // 异步加载资源
            var handleTask = GameAsset.LoadAssetAsync<GameObject>(key);
            if (!_keyToHandleTaskMap.TryAdd(key, handleTask))
            {
                handleTask = _keyToHandleTaskMap[key];
            }

            try
            {
                // 等待资源加载
                var newHandle = await handleTask;
                // 缓存Key
                _assetKeys.Add(key);
                // 缓存句柄
                _keyToHandleMap.Add(key, newHandle);
                // 实例化资源
                return Instantiate<T>(newHandle, key, parent, pos, rot, worldSpace);
            }
            catch (Exception e)
            {
                GameAsset.Release(_keyToHandleMap[key]);
                _assetKeys.Remove(key);
                _keyToHandleMap.Remove(key);
                Logger.LogError($"[{nameof(ObjectSpawner)}]: {e.Message}");
                return default;
            }
            finally
            {
                _keyToHandleTaskMap.Remove(key);
            }
        }
        
        /// 从对象池复用对象为池化对象
        private PoolObject GetPoolObject<T>(string key, Transform parent = null, 
            Vector3 pos = default, Quaternion rot = default, bool worldSpace = false) where T : Object
        {
            var instance = _poolManager.Get<T>(key);
            if (!instance) 
                return default;
            
            var poolObject = new PoolObject(ObjectIdPool.GetGlobalId(), instance, this);
            switch (instance)
            {
                case GameObject gameObject:
                {
                    if (parent)
                    {
                        gameObject.transform.SetParent(parent, worldSpace);
                        gameObject.transform.localPosition = pos;
                        gameObject.transform.localRotation = rot;
                    }
                    else
                    {
                        gameObject.transform.position = pos;
                        gameObject.transform.rotation = rot;
                    }
                    break;
                }
                case Component component:
                {
                    if (parent)
                    {
                        component.transform.SetParent(parent, worldSpace);
                        component.transform.localPosition = pos;
                        component.transform.localRotation = rot;
                    }
                    else
                    {
                        component.transform.position = pos;
                        component.transform.rotation = rot;
                    }
                    break;
                }
            }
            
            return poolObject.Convert<T>();
        }

        /// 实例化资源并封装为池化对象返回
        private PoolObject<T> Instantiate<T>(AssetHandle assetHandle, string key, Transform parent = null, 
            Vector3 pos = default, Quaternion rot = default, bool worldSpace = false) where T : Object
        {
            // 实例化资源
            var newObj = InstantiateInternal<T>(assetHandle, key, parent, pos, rot, worldSpace);
            var poolObject = new PoolObject(ObjectIdPool.GetGlobalId(), newObj, this);
            return poolObject.Convert<T>();
        }

        /// 实例化资源（内部）
        private static T InstantiateInternal<T>(AssetHandle assetHandle, string key, Transform parent = null, 
            Vector3 pos = default, Quaternion rot = default, bool worldSpace = false) where T : Object
        {
            T newObj;
            if (!parent)
            {
                newObj = Object.Instantiate(assetHandle.ConvertTo<T>().Asset, pos, rot);
            }
            else
            {
                newObj = Object.Instantiate(assetHandle.ConvertTo<T>().Asset, parent, worldSpace);
                var transform = newObj.GetComponent<Transform>();
                transform.localPosition = pos;
                transform.localRotation = rot;
            }
            
            // 修改对象名称为资源唯一Key
            newObj.name = key;
            return newObj;
        }
        
        /// <summary>
        /// 异步生成多个对象，只能获取同一类型的多个资源，不支持混合类型
        /// </summary>
        /// <param name="keys">同一类型的不同资源key</param>
        /// <typeparam name="T">类型</typeparam>
        /// <returns></returns>
        public async Task<PoolObject<T>> SpawnsAsync<T>(params string[] keys) where T : Object
        {
            var poolObject = new PoolObject(ObjectIdPool.GetGlobalId(), null, this);
            // 保存所有生成任务
            var loadTasks = new List<Task<PoolObject<T>>>();
            foreach (var key in keys)
            {
                loadTasks.Add(SpawnAsync<T>(key));
            }
            
            // 等待所有加载任务结束
            var poolObjects = await Task.WhenAll(loadTasks);
            
            // 存储结果到新池化对象中
            foreach (var po in poolObjects)
            {
                poolObject.Objs.Add(po.Obj);
            }

            return poolObject.Convert<T>();
        }

        /// <summary>
        /// 统一的回收入口（通过 PooledObject 自动调用）
        /// </summary>
        /// <param name="poolObject">池化对象</param>
        /// <param name="destroy">是否销毁不放入对象池</param>
        internal void Release(PoolObject poolObject, bool destroy)
        {
            if (poolObject.Objs.Count > 0)
            {
                foreach (var poolObjectObj in poolObject.Objs)
                {
                    ReleaseInternal(poolObjectObj, destroy);
                }
            }
            else
            {
                ReleaseInternal(poolObject.Obj, destroy);
            }
        }

        /// 释放对象（内部）
        private void ReleaseInternal(Object obj, bool destroy)
        {
            if (!obj)
            {
                Logger.LogError($"{nameof(ObjectSpawner)}: Manually destroying object is not allowed");
                return;
            }
            
            if (destroy)
                EngineUtility.Destroy(obj);
            else
            {
                Logger.Log($"[{nameof(ObjectSpawner)}]: {obj.name} collect pool");
                _poolManager.PushObj(obj);
            }
        }
        
        /// <summary>
        /// 销毁生成器，当不在使用该生成器时调用此方法，释放缓存的剩余句柄
        /// </summary>
        public void Dispose()
        {
            _poolManager.PushData(this);
        }

        void IPoolData.ResetData()
        {
            // 为了避免引用泄露，需要在不使用该生成器时统一释放剩余的句柄
            foreach (var handle in _keyToHandleMap.Values)
            {
                GameAsset.Release(handle);
            }
            _keyToHandleMap.Clear();
            
            // 清空对象池的这些资源Key的缓存对象
            foreach (var assetKey in _assetKeys)
            {
                _poolManager.ClearCache(assetKey);
            }
            _assetKeys.Clear();
        }
    }
}
