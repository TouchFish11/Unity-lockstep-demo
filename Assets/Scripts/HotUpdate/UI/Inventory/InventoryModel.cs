using System;
using System.Collections.Generic;
using Core.AssetBundles.Management;
using Core.DI;
using Core.Pool;
using Core.UI.MVC;
using HotUpdate.Base.Grid;
using HotUpdate.Common.Items;
using HotUpdate.Game.Inventory;

namespace HotUpdate.UI.Inventory
{
    /// <summary>
    /// 背包界面数据
    /// </summary>
    public class InventoryModel : UIModel
    {
        [Inject] private IPoolManager _poolManager;
        /// 缓存所有选项
        private readonly List<PoolObject> _itemTypeOpts = new();
        // 物品排序委托
        public Comparison<Item> sortComparison = InventorySorterFactory.DefaultIDSorter(1);
        
        /// <summary>
        /// 当前显示的物品类型
        /// </summary>
        public EItemType CurrentItemType { get; set; }
        
        /// <summary>
        /// 格子生成器
        /// </summary>
        public GridGenerator<Item, ItemCell> GridGenerator { get; set; }
        
        /// <summary>
        /// 详细界面工厂
        /// </summary>
        public InventoryDetailViewCreateFactory DetailPanelFactory { get; set; }

        /// <summary>
        /// 详细界面池化对象
        /// </summary>
        public PoolObject<InventoryDetailPanel> DetailPanelPoolObject { get; set; }

        public void InitDetailPanelFactory()
        {
            DetailPanelFactory = _poolManager.GetData<InventoryDetailViewCreateFactory>();
        }
        
        /// <summary>
        /// 获取第一个类型的选项
        /// </summary>
        /// <returns></returns>
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
            DetailPanelPoolObject.Collect();
            _poolManager.PushData(GridGenerator);
            _poolManager.PushData(DetailPanelFactory);
            GridGenerator = null;
            DetailPanelFactory = null;
            sortComparison = null;
            _poolManager = null;
        }
    }
}
