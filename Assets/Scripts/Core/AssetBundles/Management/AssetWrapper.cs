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
        
        /// <summary>
        /// 获取资源，同时更新AB包的访问次数
        /// </summary>
        public object Asset
        {
            get
            {
                _bundleWrapper.RecordAccess();
                return _asset;
            }
        }
        
        /// <summary>
        /// 资源是否为空
        /// </summary>
        public bool IsNull => _asset == null;
        
        public AssetWrapper(object asset, BundleWrapper bundleWrapper)
        {
            _asset = asset;
            _bundleWrapper = bundleWrapper;
        }
    }
}
