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
    public class ObjectSpawner
    {
        [Inject] private IPoolManager _poolManager;
        // 资源key到资源句柄的映射
        private readonly Dictionary<string, AssetHandle> _assetHandles = new();

        public PoolObject<T> Spawn<T>(string key, Transform parent = null, Vector3 pos = default, Quaternion rot = default, bool worldSpace = false) where T : Object
        {
            PoolObject poolObject;
            var instance = _poolManager.Get<T>(key);
            if (instance)
            {
                poolObject = new PoolObject(instance, this);
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
                
                Logger.Log($"Spawning {key}");
                return poolObject.Convert<T>();
            }
            
            if (!_assetHandles.TryGetValue(key, out var assetHandle))
            {
                assetHandle = GameAsset.LoadAsset<GameObject>(key);
                _assetHandles.TryAdd(key, assetHandle);
            }

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
            
            // 修改对象名称为资源唯一路径
            newObj.name = key;
            poolObject = new PoolObject(newObj, this);
            Logger.Log($"Spawning {key}");
            return poolObject.Convert<T>();
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
        public async Task<PoolObject<T>> SpawnAsync<T>(
            string key, Transform parent = null, Vector3 pos = default, Quaternion rot = default, bool worldSpace = false) where T : Object
        {
            PoolObject poolObject;
            var instance = _poolManager.Get<T>(key);
            if (instance)
            {
                poolObject = new PoolObject(instance, this);
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
                
                Logger.Log($"Spawning {key}");
                return poolObject.Convert<T>();
            }

            if (!_assetHandles.TryGetValue(key, out var assetHandle))
            {
                assetHandle = await GameAsset.LoadAssetAsync<GameObject>(key);
                _assetHandles.TryAdd(key, assetHandle);
            }

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
            
            // 修改对象名称为资源唯一路径
            newObj.name = key;
            poolObject = new PoolObject(newObj, this);
            Logger.Log($"Spawning {key}");
            return poolObject.Convert<T>();
        }

        /// <summary>
        /// 异步生成多个对象，只能获取同一类型的多个资源，不支持混合类型
        /// </summary>
        /// <param name="keys"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<PoolObject<T>> SpawnsAsync<T>(params string[] keys) where T : Object
        {
            var loadTasks = new List<Task<PoolObject<T>>>();
            var poolObject = new PoolObject(null, this);
            foreach (var key in keys)
            {
                var instance = _poolManager.Get<T>(key);
                if (instance)
                {
                    poolObject.Objs.Add(instance);
                }
                else
                {
                    loadTasks.Add(SpawnAsync<T>(key));
                }
            }
            
            // 等待所有加载任务结束
            var poolObjects = await Task.WhenAll(loadTasks);
            
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
            if (!poolObject.Obj)
            {
                Logger.LogError($"{nameof(ObjectSpawner)}:Manually destroying object is not allowed");
            }
            else
            {
                if(destroy)
                    EngineUtility.Destroy(poolObject.Obj);
                else
                    _poolManager.PushObj(poolObject.Obj);
            }
        }
        
        /// <summary>
        /// 清理所有句柄缓存，当不在使用该生成器时调用此方法，释放缓存的句柄
        /// </summary>
        public void ClearCache()
        {
            foreach (var handle in _assetHandles.Values)
            {
                GameAsset.Release(handle);
            }
            _assetHandles.Clear();
        }
    }
}
