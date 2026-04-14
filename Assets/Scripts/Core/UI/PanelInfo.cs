using Core.AssetBundles.Management;
using Core.UI.MVC;

namespace Core.UI
{
    /// <summary>
    /// 界面信息
    /// </summary>
    public class PanelInfo<T> : IPanelInfo where T : UIView
    {
        // 界面ID
        private int _id;
        
        /// <summary>
        /// 界面缓存对象
        /// </summary>
        public PoolObject PoolObject { get; private set; }

        public IuiModel Model { get; }
        public IuiController Controller { get; }
        public IuiView View { get; }
        
        public PanelInfo(int id, PoolObject<T> poolObject, IuiView view, IuiModel model, IuiController uIController)
        {
            _id = id;
            PoolObject = poolObject;
            View = view;
            Model = model;
            Controller = uIController;
        }
    }
}
