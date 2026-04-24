using System;
using Core.Log;

namespace Core.AssetBundles.Management
{
    /// <summary>
    /// 单个资源包装器，单个资源和其所在的包相映射
    /// </summary>
    internal class AssetWrapper
    {
        // 代表单个包的其中一个资源
        private readonly object _asset;
        // 该资源所在的AB包
        private readonly BundleWrapper _bundleWrapper;
                
        public AssetWrapper(object asset, string assetKey, BundleWrapper bundleWrapper)
        {
            _asset = asset;
            AssetKey = assetKey;
            _bundleWrapper = bundleWrapper;
        }
        
        /// <summary>
        /// 资源Key
        /// </summary>
        public string AssetKey { get; }
        
        /// <summary>
        /// 引用计数
        /// </summary>
        public uint RefCount { get; private set; }
        
        /// <summary>
        /// 获取资源，同时更新AB包的访问次数和引用计数
        /// </summary>
        public object Asset
        {
            get
            {
                Retain();
                Logger.Log($"[AssetWrapper]: '{AssetKey}' RefCount Add to: {RefCount}");
                _bundleWrapper.RecordAccess();
                return _asset;
            }
        }
        
        /// <summary>
        /// 当资源被卸载的时候，触发该回调，用于通知外部清理
        /// </summary>
        public event Action OnUnload;

        public void Retain()
        {
            ++RefCount;
        }

        public void Release()
        {
            --RefCount;
            Logger.Log($"[AssetWrapper]: '{AssetKey}' RefCount Reduce to: {RefCount}");
            if (RefCount != 0) 
                return;
            
            // 释放包引用计数
            _bundleWrapper.Release();
            OnUnload?.Invoke();
            OnUnload = null;
        }
        
        /// <summary>
        /// 资源是否为空
        /// </summary>
        public bool IsNull => _asset == null;
    }
}
