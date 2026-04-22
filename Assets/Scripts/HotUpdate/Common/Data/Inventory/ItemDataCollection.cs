using System;
using System.Collections.Generic;
using Core.Log;
using Newtonsoft.Json;

namespace HotUpdate.Common.Data.Inventory
{
    /// <summary>
    /// 物品数据集合，保存所有玩家物品数据
    /// </summary>
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class ItemDataCollection
    {
        [JsonProperty] private List<ItemData> items = new();

        public void AddItemData(ItemData itemData)
        {
            if (itemData == null)
                return;
            
            items.Add(itemData);
            Logger.Log($"{nameof(ItemDataCollection)}: Item(id = {itemData.itemId}, num = {itemData.itemNum}) added");
        }
        
        public IEnumerable<ItemData> GetItems() => items;
    }
}
