using System;
using Newtonsoft.Json;

namespace HotUpdate.Common.Data.Inventory
{
    /// <summary>
    /// 物品数据
    /// </summary>
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class ItemData
    {
        // 物品ID
        [JsonProperty] public int itemId;
        // 物品数量
        [JsonProperty] public int itemNum;
    }
}
