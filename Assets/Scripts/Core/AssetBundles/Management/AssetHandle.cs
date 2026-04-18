using System;

namespace Core.AssetBundles.Management
{
    /// <summary>
    /// 资源句柄
    /// </summary>
    public struct AssetHandle : IEquatable<AssetHandle>
    {
        /// <summary>
        /// 自身句柄唯一ID
        /// </summary>
        internal int HandleId { get; set; }

        /// <summary>
        /// 资源定位对象的版本号
        /// </summary>
        internal int Version { get; set; }

        /// <summary>
        /// 转换为泛型句柄
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public AssetHandle<T> ConvertTo<T>() where T : class
        {
            return new AssetHandle<T>(this);
        }

        public static bool operator ==(AssetHandle handle1, AssetHandle handle2) => handle1.HandleId == handle2.HandleId && handle1.Version == handle2.Version;
        public static bool operator !=(AssetHandle handle1, AssetHandle handle2) => !(handle1 == handle2);
        public override bool Equals(object obj) => obj is AssetHandle handle && Equals(handle);
        public bool Equals(AssetHandle other) => this == other;
        public override int GetHashCode() => HashCode.Combine(HandleId, Version);
    }
    
    /// <summary>
    /// 泛型资源句柄
    /// </summary>
    public readonly struct AssetHandle<T> where T : class
    {
        // 内部非泛型句柄实例
        private readonly AssetHandle _innerHandle;

        public AssetHandle(AssetHandle inner)
        {
            _innerHandle = inner;
        }
        
        /// <summary>
        /// 原始资源
        /// </summary>
        public T Asset => GameAsset.GetAsset<T>(_innerHandle.HandleId, _innerHandle.Version);

        public static implicit operator AssetHandle(AssetHandle<T> handle) => handle._innerHandle;
    }
}
