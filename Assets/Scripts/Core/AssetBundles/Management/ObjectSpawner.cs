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
            
            // 加载资源
            if (!_assetHandles.TryGetValue(key, out var assetHandle))
            {
                assetHandle = GameAsset.LoadAsset<GameObject>(key);
                _assetHandles.TryAdd(key, assetHandle);
            }
            
            // 实例化资源
            var newObj = Instantiate<T>(assetHandle, key, parent, pos, rot, worldSpace);
            poolObject = new PoolObject(newObj, this);
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
        public async Task<PoolObject<T>> SpawnAsync<T>(string key, Transform parent = null, Vector3 pos = default, Quaternion rot = default, bool worldSpace = false) where T : Object
        {
            // 尝试从对象池获取
            var poolObject = GetPoolObject<T>(key, parent, pos, rot, worldSpace);
            if(poolObject.Obj)
                return poolObject.Convert<T>();

            // 异步加载资源
            if (!_assetHandles.TryGetValue(key, out var assetHandle))
            {
                assetHandle = await GameAsset.LoadAssetAsync<GameObject>(key);
                _assetHandles.TryAdd(key, assetHandle);
            }

            // 实例化资源
            var newObj = Instantiate<T>(assetHandle, key, parent, pos, rot, worldSpace);
            poolObject = new PoolObject(newObj, this);
            return poolObject.Convert<T>();
        }
        
        /// <summary>
        /// 从对象池复用对象
        /// </summary>
        /// <param name="key"></param>
        /// <param name="parent"></param>
        /// <param name="pos"></param>
        /// <param name="rot"></param>
        /// <param name="worldSpace"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private PoolObject GetPoolObject<T>(string key, Transform parent = null, Vector3 pos = default, Quaternion rot = default, bool worldSpace = false) where T : Object
        {
            var instance = _poolManager.Get<T>(key);
            if (!instance) 
                return default;
            
            var poolObject = new PoolObject(instance, this);
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

        /// <summary>
        /// 实例化资源
        /// </summary>
        /// <param name="assetHandle"></param>
        /// <param name="key"></param>
        /// <param name="parent"></param>
        /// <param name="pos"></param>
        /// <param name="rot"></param>
        /// <param name="worldSpace"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private static T Instantiate<T>(AssetHandle assetHandle, string key, Transform parent = null, Vector3 pos = default, Quaternion rot = default, bool worldSpace = false) where T : Object
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
            
            // 修改对象名称为资源唯一路径
            newObj.name = key;
            return newObj;
        }
        
        /// <summary>
        /// 异步生成多个对象，只能获取同一类型的多个资源，不支持混合类型
        /// </summary>
        /// <param name="keys"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<PoolObject<T>> SpawnsAsync<T>(params string[] keys) where T : Object
        {
            var poolObject = new PoolObject(null, this);
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
