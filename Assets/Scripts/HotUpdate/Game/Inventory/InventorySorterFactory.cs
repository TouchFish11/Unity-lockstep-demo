using System;
using HotUpdate.Common.Items;

namespace HotUpdate.Game.Inventory
{
    /// <summary>
    /// 背包排序器工厂
    /// </summary>
    public class InventorySorterFactory
    {
        /// <summary>
        /// 默认ID排序器，默认按照物品ID降序
        /// </summary>
        /// <param name="i">正数从低到高(升序)，负数从高到低(降序)，0则不处理</param>
        /// <returns></returns>
        public static Comparison<Item> DefaultIDSorter(int i)
        {
            // 默认降序
            return i switch
            {
                0 => null,
                > 0 => (x, y) => x.itemConfig.itemId.CompareTo(y.itemConfig.itemId),    // 升序
                _ => (x, y) => y.itemConfig.itemId.CompareTo(x.itemConfig.itemId)       // 降序
            };
        }

        /// <summary>
        /// 品质类型排序器
        /// </summary>
        /// <param name="i">正数从低到高(升序)，负数从高到低(降序)，0则不处理</param>
        /// <returns></returns>
        public static Comparison<Item> QualitySorter(int i)
        {
            return i switch
            {
                0 => null,
                > 0 => (x, y) => x.itemConfig.itemQuality.CompareTo(y.itemConfig.itemQuality),    // 升序
                _ => (x, y) => y.itemConfig.itemQuality.CompareTo(x.itemConfig.itemQuality)       // 降序
            };
        }
    }
}
