using System;
using HotUpdate.Common.Config.Item;
using UnityEngine;

namespace HotUpdate.Game.Inventory
{
    /// <summary>
    /// 工具栏
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
            
            return new Color(originColor.r, originColor.g, originColor.b, 0.4f);
        }
    }
}
