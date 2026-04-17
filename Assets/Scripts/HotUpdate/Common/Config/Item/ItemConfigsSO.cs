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
        // 物品配置集合
        public ItemConfigCollection itemConfigCollection;
        
        private void OnValidate()
        {
            target = itemConfigCollection;
        }
    }
}
