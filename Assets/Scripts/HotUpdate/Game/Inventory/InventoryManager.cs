using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.AssetBundles.Management;
using Core.Pool;
using Core.Serialize.Json;
using Core.Utility;
using HotUpdate.Base;
using HotUpdate.Common.Config.Item;
using HotUpdate.Common.Data.Inventory;
using HotUpdate.Game.Data;
using UnityEngine;
using UnityEngine.U2D;

namespace HotUpdate.Game.Inventory
{
    /// <summary>
    /// 背包管理器
    /// </summary>
    public class InventoryManager : IInventoryManager
    {
        private readonly GameDataManager _gameDataManager;
        private readonly IJsonManager _jsonManager;
        private readonly IPoolManager _poolManager;
        
        public ItemConfigCollection ItemConfigCollection { get; private set; }
        
        public InventoryManager(IPoolManager poolManager, IJsonManager jsonManager, GameDataManager gameDataManager)
        {
            _poolManager = poolManager;
            _jsonManager = jsonManager;
            _gameDataManager = gameDataManager;
        }

        /// <summary>
        /// 加载物品配置
        /// </summary>
        public async Task LoadItemConfig()
        {
            var handle = await GameAsset.LoadAssetAsync<TextAsset>("");
            ItemConfigCollection = _jsonManager.FromJson<ItemConfigCollection>(handle.Asset.text, settings: NewtonsoftJsonUtility.SerializerSettings);
        }
        
        public List<ItemData> GetItemDataByType(EItemType itemType)
        {
            var list = new List<ItemData>();
            foreach (var itemData in _gameDataManager.ItemDataCollection.Items)
            {
                // 获取物品配置
                var itemConfig = ItemConfigCollection.itemConfigs.Find(x => x.itemType == itemType);
                if (itemConfig.itemType == itemType)
                {
                    list.Add(itemData);
                }
            }
            return list;
        }
        
        public async Task<ItemDTO> CreateItemDTO(ItemConfig itemConfig, ItemData itemData)
        {
            if(itemConfig == null)
                throw new ArgumentNullException($"{nameof(itemConfig)} is null");
            
            var itemDto = _poolManager.GetData<ItemDTO>();
            itemDto.itemId = itemData.itemId;
            itemDto.itemType = itemConfig.itemType;
            itemDto.iconPath = itemConfig.icon;
            itemDto.qualityType = itemConfig.itemQuality;
            // 根据是否是圣遗物决定显示数量还是等级，TODO：这里可以抽象为物品类型解析类，不同物品使用不同的解析逻辑
            itemDto.itemNumOrLv = itemConfig.itemType != EItemType.HolyRelic
                ? itemData.itemNum
                : itemData is HolyRelicData holyRelicData ? holyRelicData.level : -1;
            
            var handle = await GameAsset.LoadAssetAsync<SpriteAtlas>(itemConfig.atlasName);
            var sprite = handle.Asset.GetSprite(itemConfig.icon);
            itemDto.icon = sprite;
            itemDto.qualityBk = InventoryUtil.GetBkQualityColor(itemConfig.itemQuality);
            GameAsset.Release(handle);
            return itemDto;
        }

        public IEnumerable<ItemData> GetItems() => _gameDataManager.ItemDataCollection.Items;
    }
}
