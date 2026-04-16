using System.Collections.Generic;
using Core.SO;
using UnityEngine;

namespace HotUpdate.Common.Config.Item
{
    /// <summary>
    /// 所有物品配置SO
    /// </summary>
    [CreateAssetMenu(fileName = nameof(ItemConfigsSO), menuName = "SO/ItemConfigsSO")]
    public class ItemConfigsSO : SOBase
    {
        // 物品配置列表
        public List<ItemConfig> ItemConfigs;
        
        private void OnValidate()
        {
            target = ItemConfigs;
        }
    }
}
