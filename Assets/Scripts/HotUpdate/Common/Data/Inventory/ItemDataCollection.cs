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

        /// <summary>
        /// 在原有物品上添加数量
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="deltaNum"></param>
        public void AddItemData(int itemId, int deltaNum)
        {
            var itemData = items.Find(item => item.itemId == itemId);
            if (itemData == null)
            {
                Logger.Log($"{nameof(ItemDataCollection)}: ItemData {itemId} id not found");
                return;
            }
            
            itemData.itemNum += deltaNum;
        }

        /// <summary>
        /// 在原有物品上删除物品
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="deltaNum"></param>
        public void DeleteData(int itemId, int deltaNum)
        {
            var itemData = items.Find(item => item.itemId == itemId);
            if (itemData == null)
            {
                Logger.Log($"{nameof(ItemDataCollection)}: ItemData {itemId} id not found");
                return;
            }

            itemData.itemNum -= deltaNum;
            if (itemData.itemNum <= 0)
            {
                items.Remove(itemData);
            }
        }
        
        /// <summary>
        /// 新增新物品
        /// </summary>
        /// <param name="itemData"></param>
        public void AddData(ItemData itemData)
        {
            if (itemData == null)
                return;
            
            items.Add(itemData);
            Logger.Log($"{nameof(ItemDataCollection)}: Item(id = {itemData.itemId}, num = {itemData.itemNum}) added");
        }
        
        public IEnumerable<ItemData> GetItems() => items;
    }
}
