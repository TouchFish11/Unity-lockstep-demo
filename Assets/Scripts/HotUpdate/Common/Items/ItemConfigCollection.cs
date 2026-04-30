using System;
using System.Collections.Generic;
using HotUpdate.Common.Items.Config;
using Newtonsoft.Json;
using UnityEngine;

namespace HotUpdate.Common.Items
{
    /// <summary>
    /// 物品配置集合
    /// </summary>
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class ItemConfigCollection
    {
        [JsonProperty] [SerializeReference] public List<ItemConfig> itemConfigs = new();
    }
}
