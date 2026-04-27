using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.AssetBundles.Management;
using Core.Pool;
using HotUpdate.Base;
using HotUpdate.Common.Config.Item;
using HotUpdate.Common.Data.Inventory;
using HotUpdate.Game.Data;
using UnityEngine;
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
        
        // 实例ID映射运行时物品数据字典
        private readonly Dictionary<int, ItemData> _instanceIdToDatas =  new();
        // 物品ID到物品配置的映射
        private readonly Dictionary<int, ItemConfig> _itemConfigs = new();
        // 物品DTO映射
        private readonly Dictionary<int, ItemDTO> _instanceIdToDTOs =  new();
        // 精灵图片资源句柄缓存，唯一资源key映射句柄列表
        private readonly Dictionary<string, AssetHandle<Sprite>> _spriteToHandleMap = new();
        private readonly Dictionary<string, Task<AssetHandle<Sprite>>> _spriteToHandleTaskMap = new();
        // 物品运行时实例ID，只表示当前显示的物品实例ID，不同显示物品可复用
        private static int _instanceId;
        // 实例ID池
        private static readonly Queue<int> _instanceIds = new();
        
        public InventoryManager(IPoolManager poolManager, GameDataManager gameDataManager)
        {
            _poolManager = poolManager;
            _gameDataManager = gameDataManager;
            InitItemConfigs();
        }

        public void InitItemConfigs()
        {
            foreach (var itemConfig in _gameDataManager.ItemConfigCollection.itemConfigs)
            {
                _itemConfigs.Add(itemConfig.itemId, itemConfig);
            }
        }
        
        public ItemConfig GetItemConfig(int itemId)
        {
            return _itemConfigs.GetValueOrDefault(itemId);
        }

        public ItemData GetData(ItemDTO itemDto)
        {
            return _instanceIdToDatas.GetValueOrDefault(itemDto.instanceId);
        }

        /// <summary>
        /// 添加物品数据
        /// </summary>
        /// <param name="id"></param>
        /// <param name="num"></param>
        public void AddItemData(int id, int num)
        {
            var itemData = new ItemData
            {
                itemId = id,
                itemNum = num,
            };
            
            _gameDataManager.ItemDataCollection.AddItemData(itemData);
        }
        
        /// <summary>
        /// 更新指定类型的物品数据缓存
        /// </summary>
        /// <param name="itemType"></param>
        private void UpdateItemDataByType(EItemType itemType)
        {
            // 清空上次显示的数据缓存
            _instanceIdToDatas.Clear();
            // 回收ID
            foreach (var instanceId in _instanceIdToDTOs.Keys) PushId(instanceId);
            // 清空DTO缓存
            _instanceIdToDTOs.Clear();
            // 释放并清理显示的图标的所有句柄
            foreach (var assetHandle in _spriteToHandleMap.Values)
            {
                GameAsset.Release(assetHandle);
            }
            _spriteToHandleMap.Clear();
            // 清理正在加载的任务缓存，正常来说这里不会有遗留
            _spriteToHandleTaskMap.Clear();
            
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
                    _instanceIdToDatas.Add(GenerateInstanceId(), itemData);
                }
            }
        }

        public async Task<List<ItemDTO>> CreateItemDTOsAsync(EItemType itemType)
        {
            UpdateItemDataByType(itemType);
            // 创建所有DTO对象
            var dtoTasks = new List<Task<ItemDTO>>();
            foreach(var (instanceId, data) in _instanceIdToDatas)
            {
                var itemConfig = _itemConfigs.GetValueOrDefault(data.itemId);
                var itemData = _instanceIdToDatas.GetValueOrDefault(instanceId);
                dtoTasks.Add(CreateItemDTO(instanceId, itemConfig, itemData));
            }

            var itemDTOs = await Task.WhenAll(dtoTasks);
            // 等待所有DTO对象创建完成
            return new List<ItemDTO>(itemDTOs);
        }
        
        public async Task<ItemDTO> CreateItemDTO(int instanceId, ItemConfig itemConfig, ItemData itemData)
        {
            var itemDto = _poolManager.GetData<ItemDTO>();
            itemDto.itemId = itemData.itemId;
            itemDto.itemType = itemConfig.itemType;
            itemDto.iconPath = itemConfig.icon;
            itemDto.qualityType = itemConfig.itemQuality;
            // 根据是否是圣遗物决定显示数量还是等级，TODO：这里可以抽象为物品类型解析类，不同物品使用不同的解析逻辑
            itemDto.itemNumOrLv = itemConfig.itemType != EItemType.HolyRelic
                ? itemData.itemNum
                : itemData is HolyRelicData holyRelicData ? holyRelicData.level : -1;
            // 设置背景
            itemDto.qualityBk = InventoryUtil.GetBkQualityColor(itemConfig.itemQuality);
            // 设置实例ID
            itemDto.instanceId = instanceId;
            // 缓存DTO
            _instanceIdToDTOs.Add(instanceId, itemDto);
            
            // 查找句柄缓存
            if (_spriteToHandleMap.TryGetValue(itemConfig.icon, out var assetHandle))
            {
                itemDto.icon = assetHandle.Asset;
                return itemDto;
            }
            
            // 正在加载，返回同一个加载任务
            if (_spriteToHandleTaskMap.TryGetValue(itemConfig.icon, out var cacheTask))
            {
                var handle = await cacheTask;
                itemDto.icon = handle.Asset;
                return itemDto;
            }

            // 首次加载资源
            var newTask = GameAsset.LoadAssetAsync<Sprite>(itemConfig.icon);
            // 缓存正在加载的资源任务
            if (!_spriteToHandleTaskMap.TryAdd(itemConfig.icon, newTask))
            {
                newTask = _spriteToHandleTaskMap[itemConfig.icon];
            }

            try
            {
                var newHandle = await newTask;
                // 设置图标
                itemDto.icon = newHandle.Asset;
                // 缓存句柄
                _spriteToHandleMap.Add(itemConfig.icon, newHandle);
                return itemDto;
            }
            catch (Exception e)
            {
                Logger.LogError($"[{nameof(InventoryManager)}]: '{itemConfig.icon}' asset load fail, {e.Message}");
                // 加载失败，使用默认资源替代
                // itemDto.icon = iconSprite;
                return itemDto;
            }
            finally
            {
                // 移除正在加载的任务
                _spriteToHandleTaskMap.Remove(itemConfig.icon);
            }
        }

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
        /// 缓存实例ID
        /// </summary>
        /// <param name="instanceId"></param>
        private static void PushId(int instanceId)
        {
            _instanceIds.Enqueue(instanceId);
        }
    }
}
