using System.Threading.Tasks;
using Core.AssetBundles.Management;
using Core.DI;
using Core.Singleton;
using UnityEngine;

namespace Core.PreLoad
{
    /// <summary>
    /// 预加载管理器
    /// </summary>
    public class PreLoadManager : IPreLoadManager, IInitializable
    {
        [Inject] private IAssetBundleManager _assetBundleManager;
        public int InitPriority => 0;

        private PreLoadManager()
        {
        
        }

        public Task InitAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// 预加载资源
        /// </summary>
        /// <param name="preLoadDatas">预加载数据</param>
        public async Task PreLoads(params PreLoadData[] preLoadDatas)
        {
            foreach (var preLoadData in preLoadDatas)
            {
                await GameAsset.LoadAssetAsync<Object>(preLoadData.AssetName);
            }
        }
    }
}
