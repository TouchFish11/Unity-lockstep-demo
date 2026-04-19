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
        /// <param name="abName">AB包名称（不含拓展名）</param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<BundleWrapper> LoadBundleAsync(string abName, CancellationToken token = default);

        /// <summary>
        /// 卸载所有已加载的AssetBundle
        /// </summary>
        /// <param name="unloadAllObjects"></param>
        Task UnloadAllBundles(bool unloadAllObjects);
        
        /// <summary>
        /// 释放指定包的所有依赖包，用于减少依赖项的引用计数
        /// </summary>
        /// <param name="abName">AB包名称（不含拓展名）</param>
        void ReleaseDependencies(string abName);
    }
}
