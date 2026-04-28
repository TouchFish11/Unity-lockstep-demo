using System;
using Newtonsoft.Json;

namespace HotUpdate.Common.Data.Inventory
{
    /// <summary>
    /// 物品数据
    /// </summary>
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class ItemData
    {
        // 物品ID
        [JsonProperty] public int itemId;
        // 物品数量
        [JsonProperty] public int itemNum;
        // 是否是新获取
        [JsonProperty] public bool isNew;
    }
}
