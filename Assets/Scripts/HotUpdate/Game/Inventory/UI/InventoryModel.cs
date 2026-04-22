using System.Collections.Generic;
using Core.AssetBundles.Management;
using Core.DI;
using Core.Pool;
using Core.UI.MVC;
using HotUpdate.Base.Grid;
using HotUpdate.Common.Config.Item;

namespace HotUpdate.Game.Inventory.UI
{
    /// <summary>
    /// 背包界面数据
    /// </summary>
    public class InventoryModel : UIModel
    {
        [Inject] private IPoolManager _poolManager;
        /// 缓存所有选项
        private readonly List<PoolObject> _itemTypeOpts = new();
        
        /// <summary>
        /// 格子生成器
        /// </summary>
        public GridGenerator<ItemDTO, ItemCell> GridGenerator { get; set; }

        /// <summary>
        /// 详细界面
        /// </summary>
        public IInventoryDetailPanel InventoryDetailPanel { get; set; }

        /// <summary>
        /// 详细界面池化对象
        /// </summary>
        public PoolObject DetailPanelPoolObject { get; set; }
        
        public ItemTypeOpt GetFirstItemTypeOpt()
        {
            return _itemTypeOpts[0].Convert<ItemTypeOpt>().Obj;
        }

        public void AddItemTypeOpt(PoolObject itemTypeOpt)
        {
            _itemTypeOpts.Add(itemTypeOpt);   
        }
        
        /// <summary>
        /// 清空选项
        /// </summary>
        public void ClearOpt()
        {
            foreach (var itemTypeOpt in _itemTypeOpts)
            {
                itemTypeOpt.Collect();
            }
            _itemTypeOpts.Clear();
        }
        
        public override void ClearData()
        {
            ClearOpt();
            _poolManager.PushData(GridGenerator);
            InventoryDetailPanel = null;
            DetailPanelPoolObject.Collect();
        }
    }
}
