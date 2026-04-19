namespace Core.AssetBundles.Management
{
    internal class AssetWrapper
    {
        // 代表单个包的一个资源或资源列表List
        private readonly object _asset;
        private readonly BundleWrapper _bundleWrapper;
        
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
