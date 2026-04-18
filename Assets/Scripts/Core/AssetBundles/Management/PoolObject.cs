using System;
using Object = UnityEngine.Object;

namespace Core.AssetBundles.Management
{
    public struct PoolObject : IDisposable
    {
        private ObjectSpawner _spawner;
        
        internal Object Obj { get; private set; }

        public PoolObject(Object obj, ObjectSpawner spawner)
        {
            Obj = obj;
            _spawner = spawner;
        }
        /// <summary>
        /// 回收对象，内部游戏对象实例回收到缓存池中
        /// </summary>
        public void Collect()
        {
            _spawner.Release(Obj);
            Obj = null;
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
    public struct PoolObject<T> : IDisposable where T : Object
    {
        private PoolObject _innerObject;

        public T Obj => _innerObject.Obj as T;
        
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
