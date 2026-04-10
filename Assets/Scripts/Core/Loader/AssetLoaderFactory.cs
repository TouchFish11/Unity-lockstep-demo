using Core.HotUpdate;
using Core.Reflection;

namespace Core.Loader
{
    /// <summary>
    /// 资源加载器工厂
    /// 新增加载器时需要手动注册
    /// </summary>
    public class AssetLoaderFactory : Factory<IAssetLoader>
    {
        private readonly IHotUpdateManager _hotUpdateManager;
        
        private AssetLoaderFactory(IHotUpdateManager hotUpdateManager)
        {
            _hotUpdateManager = hotUpdateManager;
        }
        
        public override void InitFactory()
        {
            FactoryUtility.ScanAllType(typeToInterfaceMap, _hotUpdateManager.GetCoreModule());
            
            // 注册加载器到依赖容器中
            foreach (var assetLoader in typeToInterfaceMap.Values)
            {
                switch (assetLoader)
                {
                    // case ISpriteLoader spriteLoader:
                    //     DIContainer.InjectInstance(spriteLoader);
                    //     break;
                    // case IAudioLoader audioLoader:
                    //     DIContainer.InjectInstance(audioLoader);
                    //     break;
                    // case IPrefabLoader prefabLoader:
                    //     DIContainer.InjectInstance(prefabLoader);
                    //     break;
                }
            }
        }
    }
}
