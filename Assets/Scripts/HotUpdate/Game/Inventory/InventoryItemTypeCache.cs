using System.Collections.Generic;
using HotUpdate.Common.Items;
using HotUpdate.Common.Items.Data;

namespace HotUpdate.Game.Inventory
{
    /// <summary>
    /// 背包物品类型缓存
    /// </summary>
    public class InventoryItemTypeCache
    {
        public Dictionary<EItemType, List<ItemData>> TypeToDataMap { get; } = new();


        /// <summary>
        /// 初始化缓存
        /// </summary>
        /// <param name="itemType"></param>
        /// <param name="itemData"></param>
        public void InitCache(EItemType itemType, ItemData itemData)
        {
            if (TypeToDataMap.TryGetValue(itemType, out var data))
            {
                data.Add(itemData);
            }
            else
            {
                TypeToDataMap.Add(itemType, new List<ItemData> { itemData });
            }
        }
        
    }
}
