using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace HotUpdate.Common.Config.Item
{
    /// <summary>
    /// 物品配置集合
    /// </summary>
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class ItemConfigCollection
    {
        [JsonProperty] public List<ItemConfig> itemConfigs = new();
    }
}
