using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Pool;
using HotUpdate.Base.Icon;
using HotUpdate.Base.Inventory;
using HotUpdate.Base.Items;
using HotUpdate.Common.Items;
using HotUpdate.Common.Items.Config;
using HotUpdate.Common.Items.Data;
using HotUpdate.Game.Data;

namespace HotUpdate.Game.Inventory
{
    /// <summary>
    /// 背包管理器
    /// </summary>
    public class InventoryManager : IInventoryManager
    {
        private readonly ItemDataProvider _itemDataProvider;
        private readonly IPoolManager _poolManager;
        private readonly IIconProvider _iconProvider;
        
        // 当前显示的物品持久化ID到物品对象的映射
        private readonly Dictionary<long, Item> _instanceIdToItemMap =  new();
        // 当前已经加载过的图标Key
        private readonly HashSet<string> _iconKeys = new();
        
        public InventoryManager(IPoolManager poolManager, GameDataManager gameDataManager, IIconProvider iconProvider)
        {
            _poolManager = poolManager;
            _iconProvider = iconProvider;
            _itemDataProvider = gameDataManager.ItemDataProvider;
        }

        public IEnumerable<Item> GetAllItems()
        {
            return _instanceIdToItemMap.Values;
        }

        /// <summary>
        /// 删除可堆叠的物品
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="deleteNum"></param>
        public void DeleteItem(int itemId, int deleteNum)
        {
            _itemDataProvider.RemoveData(itemId, deleteNum, true);
        }
        
        /// <summary>
        /// 删除不可堆叠物品
        /// </summary>
        /// <param name="persistentId"></param>
        public void DeleteItem(long persistentId)
        {
            _itemDataProvider.RemoveData(persistentId, 1, false);
        }
        
        public ItemData GetData(Item item)
        {
            // 根据是否可堆叠查找不同的物品数据
            if (item.itemConfig.isPile)
                return _itemDataProvider.TryGetData(item.itemConfig.itemId,  out var itemData) ? itemData : null;
            return _itemDataProvider.TryGetInstanceData(item.persistentId, out var data) ? data : null;
        }

        public void UpdateGridNewState(Item item)
        {
            item.isNew = false;
            var itemData = GetData(item);
            if (itemData != null && itemData.isNew)
                itemData.isNew = false;
        }
        
        public async Task<List<Item>> CreateItemsAsync(EItemType itemType)
        {
            // 回收Item
            foreach (var (_, item) in _instanceIdToItemMap)
            {
                _poolManager.PushData(item);
            }
            // 清空缓存
            _instanceIdToItemMap.Clear();
            
            // 创建所有物品对象
            var dtoTasks = new List<Task<Item>>();
            foreach(var itemData in _itemDataProvider.GetItemsByType(itemType))
            {
                var itemConfig = _itemDataProvider.ConfigMap.GetValueOrDefault(itemData.itemId);
                dtoTasks.Add(CreateItem(itemData.persistentId, itemConfig, itemData));
            }

            var itemDTOs = await Task.WhenAll(dtoTasks);
            // 等待所有对象创建完成
            return new List<Item>(itemDTOs);
        }

        public void Clear()
        {
            // 释放已经加载过的图片资源句柄
            foreach (var iconKey in _iconKeys)
            {
                // 释放显示的图标的句柄
                _iconProvider.Release(iconKey);
            }
            _iconKeys.Clear();
            _instanceIdToItemMap.Clear();
        }
        
        /// <summary>
        /// 创建物品对象
        /// </summary>
        /// <param name="persistentId"></param>
        /// <param name="itemConfig"></param>
        /// <param name="itemData"></param>
        /// <returns></returns>
        private async Task<Item> CreateItem(long persistentId, ItemConfig itemConfig, ItemData itemData)
        {
            // 对象池复用对象
            var item = _poolManager.GetData<Item>();
            // 设置实例ID
            item.persistentId = persistentId;
            // 引用物品配置
            item.itemConfig = itemConfig;
            // 根据物品类型决定显示什么数值
            item.auxValue = ItemResolver.ResolveAux(itemData);
            // 是否是新物品
            item.isNew = itemData.isNew;
            // 缓存对象
            if(persistentId != ItemPersistentIdGenerator.DefaultNotStackableId)
                _instanceIdToItemMap.Add(persistentId, item);
            // 加载该物品的图标
            var sprite = await _iconProvider.LoadIconAsync(itemConfig.icon);
            // 缓存加载成功物品图标Key
            if (sprite)
                _iconKeys.Add(itemConfig.icon);
            return item;
        }
    }
}
