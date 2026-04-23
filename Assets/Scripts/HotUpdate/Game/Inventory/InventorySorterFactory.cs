using System;
using HotUpdate.Common.Config.Item;

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
        public static Comparison<ItemDTO> DefaultIDSorter(int i)
        {
            // 默认降序
            return i switch
            {
                0 => null,
                > 0 => (x, y) => x.itemId.CompareTo(y.itemId),    // 升序
                _ => (x, y) => y.itemId.CompareTo(x.itemId)       // 降序
            };
        }

        /// <summary>
        /// 品质类型排序器
        /// </summary>
        /// <param name="i">正数从低到高(升序)，负数从高到低(降序)，0则不处理</param>
        /// <returns></returns>
        public static Comparison<ItemDTO> QualitySorter(int i)
        {
            return i switch
            {
                0 => null,
                > 0 => (x, y) => x.qualityType.CompareTo(y.qualityType),    // 升序
                _ => (x, y) => y.qualityType.CompareTo(x.qualityType)       // 降序
            };
        }
    }
}
