using System;
using System.Collections.Generic;

namespace Core.AssetBundles.Management
{
    /// <summary>
    /// 复合句柄
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Obsolete]
    public class BatchHandle<T> where T : UnityEngine.Object
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
        
        internal void Release()
        {
            foreach (var handle in _handles)
            {
                //handle.Release();
            }
            
            Assets.Clear();
            _handles.Clear();
        }
    }
}
