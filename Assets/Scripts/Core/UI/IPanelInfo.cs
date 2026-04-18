using Core.AssetBundles.Management;
using Core.UI.MVC;

namespace Core.UI
{
    public interface IPanelInfo
    {
        /// <summary>
        /// 界面缓存对象
        /// </summary>
        PoolObject PoolObject { get; }
        
        /// <summary>
        /// 界面数据接口
        /// </summary>
        IuiModel Model { get; }
        
        /// <summary>
        /// 界面控制器接口
        /// </summary>
        IuiController Controller { get; }
        
        /// <summary>
        /// 界面视图接口
        /// </summary>
        IuiView View { get; }
    }
}
