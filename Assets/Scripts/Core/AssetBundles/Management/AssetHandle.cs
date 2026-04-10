using System;

namespace Core.AssetBundles.Management
{
    /// <summary>
    /// 资源句柄基类
    /// </summary>
    public abstract class AssetHandle
    {
        /// <summary>
        /// 资源释放回调
        /// </summary>
        internal Action release;
        
        /// <summary>
        /// 资源引用计数
        /// </summary>
        protected uint ReferenceCount { get; set; }

        /// <summary>
        /// 转换为泛型句柄
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T ConvertTo<T>()  where T : class
        {
            return this as T;
        }
        
        /// <summary>
        /// 添加引用计数，当资源被多处使用时
        /// </summary>
        internal void Retain()
        {
            ++ReferenceCount;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        internal abstract void Release();
    }
    
    /// <summary>
    /// 资源句柄
    /// </summary>
    public class AssetHandle<T> : AssetHandle where T : UnityEngine.Object
    {
        /// <summary>
        /// 资源
        /// </summary>
        public T Asset { get; internal set; }

        /// <summary>
        /// 释放资源
        /// </summary>
        internal override void Release()
        {
            if (ReferenceCount > 0)
            {
                --ReferenceCount;
            }

            if (ReferenceCount != 0)
            {
                return;
            }
            
            release?.Invoke();
            release = null;
            Asset = null;
        }
    }
}
