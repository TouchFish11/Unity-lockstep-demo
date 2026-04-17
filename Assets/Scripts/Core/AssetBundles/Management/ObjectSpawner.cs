using System.Collections.Generic;
using System.Threading.Tasks;
using Core.DI;
using Core.Pool;
using UnityEngine;
using Logger = Core.Log.Logger;

namespace Core.AssetBundles.Management
{
    /// <summary>
    /// 对象生成器
    /// </summary>
    public class ObjectSpawner
    {
        [Inject] private IPoolManager _poolManager;
        private readonly Dictionary<string, AssetHandle> _assetHandles = new();
        
        /// <summary>
        /// 统一的生成入口
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<PoolObject<T>> SpawnAsync<T>(
            string key, Transform parent = null, Vector3 pos = default, Quaternion rot = default, bool worldSpace = false) where T : Object
        {
            PoolObject poolObject;
            var instance = _poolManager.Get(key);
            if (instance)
            {
                poolObject = new PoolObject(instance as T, this);
                return poolObject.Convert<T>();
            }

            if (!_assetHandles.TryGetValue(key, out var assetHandle))
            {
                // 底层只负责加载Prefab，实例化由中间层控制
                assetHandle = await GameAsset.LoadAssetAsync<T>(key);
                _assetHandles.Add(key, assetHandle);
            }

            T newObj;
            if (!parent)
            {
                newObj = Object.Instantiate(assetHandle.ConvertTo<T>().Asset, pos, rot);
            }
            else
            {
                newObj = Object.Instantiate(assetHandle.ConvertTo<T>().Asset, parent, worldSpace);
                if (newObj is Transform transform)
                {
                    transform.localPosition = pos;
                    transform.localRotation = rot;
                }
            }
            
            // 修改名称
            newObj.name = key;
            poolObject = new PoolObject(newObj, this);
            return poolObject.Convert<T>();
        }

        /// <summary>
        /// 统一的回收入口（通过 PooledObject 自动调用）
        /// </summary>
        /// <param name="obj">游戏对象</param>
        internal void Release(GameObject obj)
        {
            if (!obj)
            {
                Logger.LogError($"{nameof(ObjectSpawner)}:Manually destroying object is not allowed");
                return;
            }
            
            _poolManager.PushObj(obj);
        }

        /// <summary>
        /// 清理所有句柄缓存，当不在使用该生成器时调用此方法，释放缓存的句柄
        /// </summary>
        public void ClearCache()
        {
            _assetHandles.Clear();
        }
    }
}
