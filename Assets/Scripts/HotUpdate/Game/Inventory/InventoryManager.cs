using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Pool;
using HotUpdate.Base.Icon;
using HotUpdate.Base.Inventory;
using HotUpdate.Common.Config.Item;
using HotUpdate.Common.Data.Inventory;
using HotUpdate.Game.Data;
using Logger = Core.Log.Logger;

namespace HotUpdate.Game.Inventory
{
    /// <summary>
    /// 背包管理器
    /// </summary>
    public class InventoryManager : IInventoryManager
    {
        private readonly GameDataManager _gameDataManager;
        private readonly IPoolManager _poolManager;
        private readonly IIconProvider _iconProvider;
        
        // 物品ID到物品配置的映射
        private readonly Dictionary<int, ItemConfig> _itemConfigs = new();
        // 正在显示的物品实例ID映射运行时物品数据字典
        private readonly Dictionary<int, ItemData> _instanceIdToDataMap = new();
        // 物品运行时ID到物品对象的映射，不同运行时对象有唯一ID
        private readonly Dictionary<int, Item> _instanceIdToItemMap =  new();
        // 物品运行时实例ID，只表示当前显示的物品实例ID，不同显示物品可复用
        private static int _instanceId;
        // 实例ID池
        private static readonly Queue<int> _instanceIds = new();
        // 当前已经加载过的图标Key
        private readonly HashSet<string> _iconKeys = new();
        
        public InventoryManager(IPoolManager poolManager, GameDataManager gameDataManager, IIconProvider iconProvider)
        {
            _poolManager = poolManager;
            _gameDataManager = gameDataManager;
            _iconProvider = iconProvider;
            InitItems();
        }

        /// <summary>
        /// 初始化物品
        /// </summary>
        private void InitItems()
        {
            // 缓存所有物品配置数据
            foreach (var itemConfig in _gameDataManager.ItemConfigCollection.itemConfigs)
            {
                _itemConfigs.Add(itemConfig.itemId, itemConfig);
            }
        }
        
        public ItemConfig GetItemConfig(int itemId)
        {
            return _itemConfigs.GetValueOrDefault(itemId);
        }

        public ItemData GetData(int instanceId)
        {
            return _instanceIdToDataMap.GetValueOrDefault(instanceId);
        }
        
        public void AddData(int id, int deltaNum)
        {
            // 获取该ID的物品配置
            if (!_itemConfigs.TryGetValue(id, out var itemConfig))
            {
                Logger.LogError($"[{nameof(InventoryManager)}]: Item {id} id not found");
                return;
            }

            // 不存在数据，或不可堆叠，新增数据
            if (!_gameDataManager.ItemDataCollection.TryGetData(id, out var itemData))
            {
                if (itemConfig.isPile)
                {
                    itemData = new ItemData
                    {
                        itemId = id,
                        itemNum = deltaNum,
                    };
                }
                else
                {
                    itemData = new ItemData
                    {
                        itemId = id,
                        itemNum = 1,
                    };
                }

                _gameDataManager.ItemDataCollection.AddData(itemData);
            }
            // 存在数据且可堆叠，添加数量即可
            else
            {
                itemData.itemNum += deltaNum;
            }
        }
        
        public void DeleteData(int itemId, int num)
        {
            _gameDataManager.ItemDataCollection.DeleteData(itemId, num);
        }
        
        public async Task<List<Item>> CreateItemsAsync(EItemType itemType)
        {
            UpdateItemDataByType(itemType);
            // 创建所有物品对象
            var dtoTasks = new List<Task<Item>>();
            foreach(var (instanceId, data) in _instanceIdToDataMap)
            {
                var itemConfig = _itemConfigs.GetValueOrDefault(data.itemId);
                var itemData = _instanceIdToDataMap.GetValueOrDefault(instanceId);
                dtoTasks.Add(CreateItem(instanceId, itemConfig, itemData));
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
            
            // 清理配置数据缓存
            _itemConfigs.Clear();
            _instanceIdToDataMap.Clear();
            _instanceIdToItemMap.Clear();
        }
        
        /// <summary>
        /// 更新指定类型的物品数据缓存
        /// </summary>
        /// <param name="itemType"></param>
        private void UpdateItemDataByType(EItemType itemType)
        {
            // 清空上次显示的数据缓存
            _instanceIdToDataMap.Clear();
            // 回收ID和回收Item
            foreach (var (instanceId, item) in _instanceIdToItemMap)
            {
                PushId(instanceId);
                _poolManager.PushData(item);
            }
            // 清空缓存
            _instanceIdToItemMap.Clear();
            
            foreach (var itemData in _gameDataManager.ItemDataCollection.GetItems())
            {
                // 获取物品配置
                var itemConfig = _itemConfigs.GetValueOrDefault(itemData.itemId);
                if (itemConfig == null)
                {
                    Logger.LogWarning($"Item {itemData.itemId} not found");
                    continue;
                }
                
                if (itemConfig.itemType == itemType)
                {
                    _instanceIdToDataMap.Add(GenerateInstanceId(), itemData);
                }
            }
        }
        
        /// <summary>
        /// 创建物品对象
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="itemConfig"></param>
        /// <param name="itemData"></param>
        /// <returns></returns>
        private async Task<Item> CreateItem(int instanceId, ItemConfig itemConfig, ItemData itemData)
        {
            // 对象池复用对象
            var item = _poolManager.GetData<Item>();
            // 设置实例ID
            item.instanceId = instanceId;
            // 引用物品配置
            item.itemConfig = itemConfig;
            // 根据物品类型决定显示什么数值
            item.auxValue = ItemResolver.ResolveAux(itemData);
            // 是否是新物品
            item.isNew = itemData.isNew;
            // 缓存对象
            _instanceIdToItemMap.Add(instanceId, item);
            // 加载该物品的图标
            var sprite = await _iconProvider.LoadIconAsync(itemConfig.icon);
            // 缓存加载成功物品图标Key
            if (sprite)
                _iconKeys.Add(itemConfig.icon);
            return item;
        }
        
        /// <summary>
        /// 获取玩家所有物品数据
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ItemData> GetItems() => _gameDataManager.ItemDataCollection.GetItems();
        
        /// <summary>
        /// 生成实例ID
        /// </summary>
        /// <returns></returns>
        private static int GenerateInstanceId()
        {
            return _instanceIds.TryDequeue(out var id) ? id : _instanceId++;
        }

        /// <summary>
        /// 回收实例ID
        /// </summary>
        /// <param name="instanceId"></param>
        private static void PushId(int instanceId)
        {
            _instanceIds.Enqueue(instanceId);
        }
    }
}
