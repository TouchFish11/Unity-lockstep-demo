using System;
using System.Collections.Generic;
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
        
        public List<ItemData> Items => items;
    }
}
