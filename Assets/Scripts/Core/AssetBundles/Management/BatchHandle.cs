using System.Collections.Generic;

namespace Core.AssetBundles.Management
{
    /// <summary>
    /// 复合句柄
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BatchHandle<T> : AssetHandle where T : UnityEngine.Object
    {
        private readonly List<AssetHandle<T>> _handles;

        public List<T> Assets { get; } = new();

        public BatchHandle(IEnumerable<AssetHandle<T>> handles)
        {
            _handles = new List<AssetHandle<T>>(handles);
            foreach (var handle in _handles)
            {
                Assets.Add(handle.Asset);
            }
        }
        
        internal override void Release()
        {
            foreach (var handle in _handles)
            {
                handle.Release();
            }
            
            Assets.Clear();
            _handles.Clear();
        }
    }
}
