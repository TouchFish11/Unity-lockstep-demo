using System.Collections.Generic;
using Core.AssetBundles.Management;
using Core.UI.MVC;

namespace HotUpdate.Game.Inventory.UI
{
    /// <summary>
    /// 背包界面数据
    /// </summary>
    public class InventoryModel : UIModel
    {
        /// 缓存所有选项
        private readonly List<PoolObject> _itemTypeOpts = new();
        
        /// 当前分类显示的所有物品格子缓存
        private readonly List<PoolObject> _currentItems =  new();

        public ItemTypeOpt GetFirstItemTypeOpt()
        {
            return _itemTypeOpts[0].Convert<ItemTypeOpt>().Obj;
        }
        
        public void AddItem(PoolObject poolObject)
        {
            _currentItems.Add(poolObject);
        }

        public ItemCell GetFirstItem()
        {
            return _currentItems[0].Convert<ItemCell>().Obj;
        }

        public void ClearItems()
        {
            foreach (var currentItem in _currentItems)
            {
                currentItem.Collect();
            }
            _currentItems.Clear();
        }
        
    }
}
