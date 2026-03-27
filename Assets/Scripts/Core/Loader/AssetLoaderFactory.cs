using Core.DI;
using Core.HotUpdate;
using Core.Loader.Audio;
using Core.Loader.Object;
using Core.Loader.Sprite;
using Core.Reflection;
using Core.Service;

namespace Core.Loader
{
    /// <summary>
    /// 资源加载器工厂
    /// 新增加载器时需要手动注册
    /// </summary>
    public class AssetLoaderFactory : Factory<IAssetLoader>
    {
        public override void InitFactory()
        {
            FactoryUtility.ScanAllType(typeToInterfaceMap, ServiceLocator.Get<IHotUpdateManager>().GetCoreModule());
            
            // 注册加载器到依赖容器中
            foreach (var assetLoader in typeToInterfaceMap.Values)
            {
                switch (assetLoader)
                {
                    case ISpriteLoader spriteLoader:
                        DIContainer.InjectInstance(spriteLoader);
                        break;
                    case IAudioLoader audioLoader:
                        DIContainer.InjectInstance(audioLoader);
                        break;
                    case IPrefabLoader prefabLoader:
                        DIContainer.InjectInstance(prefabLoader);
                        break;
                }
            }
        }
    }
}
