using Core.AssetBundles.Management;
using Core.UI.MVC;

namespace Core.UI
{
    public interface IPanelInfo
    {
        PoolObject PoolObject { get; }
        
        IuiModel Model { get; }
        
        IuiController Controller { get; }
        
        IuiView View { get; }
    }
}
