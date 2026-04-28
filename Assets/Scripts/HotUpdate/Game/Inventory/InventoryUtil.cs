using System;
using HotUpdate.Common.Config.Item;
using HotUpdate.Common.Data.Inventory;
using UnityEngine;

namespace HotUpdate.Game.Inventory
{
    /// <summary>
    /// 背包工具栏
    /// </summary>
    public static class InventoryUtil
    {
        /// <summary>
        /// 物品配置转背景颜色
        /// </summary>
        /// <param name="qualityType"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static Color GetBkQualityColor(EItemQuality qualityType)
        {
            var originColor = qualityType switch
            {
                EItemQuality.Normal => Color.gray,
                EItemQuality.Rare => Color.blue,
                EItemQuality.Epitome => Color.magenta,
                EItemQuality.Legend => Color.yellow,
                EItemQuality.Immortality => Color.red,
                _ => throw new ArgumentOutOfRangeException(nameof(qualityType), qualityType, null)
            };
            
            return new Color(originColor.r, originColor.g, originColor.b, 0.5f);
        }

        /// <summary>
        /// 获取物品数据的数量或等级
        /// </summary>
        /// <param name="data"></param>
        /// <param name="itemType"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static int GetItemNumOrLevel(ItemData data, EItemType itemType)
        {
            // 根据物品类型决定显示数量还是等级
            return itemType switch
            {
                EItemType.Material or EItemType.precious => data.itemNum,
                EItemType.Weapon or EItemType.HolyRelic => ((HolyRelicData)data).level,
                _ => throw new ArgumentOutOfRangeException(nameof(itemType), itemType, null)
            };
        }
    }
}
