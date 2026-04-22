using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace Core.AssetBundles.Management
{
    /// <summary>
    /// 池化对象
    /// </summary>
    public struct PoolObject : IDisposable
    {
        // 对象生成器
        private ObjectSpawner _spawner;
        // 对象实例
        public Object Obj { get; }
        // 对象实例列表
        public List<Object> Objs { get; private set; }

        public PoolObject(Object obj, ObjectSpawner spawner)
        {
            Obj = obj;
            _spawner = spawner;
            Objs = new List<Object>();
        }

        /// <summary>
        /// 回收对象，内部游戏对象实例回收到缓存池中
        /// </summary>
        /// <param name="isDestroy">是否销毁，不回收到对象池中</param>
        public void Collect(bool isDestroy = false)
        {
            if (Obj)
            {
                _spawner.Release(this, isDestroy);
            }
            else
            {
                foreach (var obj in Objs)
                {
                    _spawner.Release(this, isDestroy);
                }
                
                Objs.Clear();
                Objs = null;
            }
            
            _spawner = null;
        }
        
        void IDisposable.Dispose()
        {
            Collect();
        }

        public PoolObject<T> Convert<T>() where T : Object
        {
            return new PoolObject<T>(this);
        }
    }
    
    /// <summary>
    /// 缓存池泛型对象，对游戏对象的封装
    /// </summary>
    public struct PoolObject<T> : IDisposable where T : class
    {
        private PoolObject _innerObject;

        public T Obj => _innerObject.Obj as T;

        public IList<T> Objs => _innerObject.Objs.ConvertAll(o => o as T);
        
        public PoolObject(PoolObject inner)
        {
            _innerObject = inner;
        }

        /// <summary>
        /// 回收对象，内部游戏对象实例回收到缓存池中
        /// </summary>
        public void Collect()
        {
            _innerObject.Collect();
        }

        void IDisposable.Dispose()
        {
            ((IDisposable)_innerObject).Dispose();
        }
        
        public static implicit operator PoolObject(PoolObject<T> poolObject) => poolObject._innerObject;
    }
}
