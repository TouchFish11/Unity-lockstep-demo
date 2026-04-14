using System.Threading;
using System.Threading.Tasks;
using Core.Systems.Memorys;

namespace Core.AssetBundles.Management
{
    /// <summary>
    /// AB包管理器接口
    /// </summary>
    internal interface IAssetBundleManager : IMemoryListener
    {
        /// <summary>
        /// 资源目录
        /// </summary>
        AssetCatalog Catalog { get; }
        
        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns>是否初始化成功</returns>
        Task Init();

        /// <summary>
        /// 异步加载指定AB包
        /// </summary>
        /// <param name="abName"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<BundleWrapper> LoadBundleAsync(string abName, CancellationToken token = default);

        /// <summary>
        /// 卸载所有已加载的AssetBundle
        /// 调用该方法后，若需要加载AB包，需重新初始化（Init）管理器
        /// </summary>
        /// <param name="unloadAllObjects"></param>
        Task UnloadAllBundles(bool unloadAllObjects);

        /// <summary>
        /// 初始化默认包
        /// 更新使用
        /// </summary>
        /// <param name="abNames"></param>
        Task InitSpecifyAsync(params string[] abNames);
        
        /// <summary>
        /// 释放指定包的依赖包，用于减少依赖项的引用计数
        /// </summary>
        /// <param name="abName"></param>
        void ReleaseDependencies(string abName);
    }
}
