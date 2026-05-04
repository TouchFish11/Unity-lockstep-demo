using System.Collections.Generic;
using System.Threading.Tasks;
using HotUpdate.Common.Items;
using HotUpdate.Common.Items.Data;

namespace HotUpdate.Base.Inventory
{
    public interface IInventoryManager
    {
        /// <summary>
        /// 异步创建物品对象
        /// </summary>
        /// <param name="itemType"></param>
        /// <returns></returns>
        Task<List<Item>> CreateItemsAsync(EItemType itemType);

        /// <summary>
        /// 通过物品实例ID获取物品数据
        /// </summary>
        /// <param name="item">物品对象</param>
        /// <returns></returns>
        ItemData GetData(Item item);

        /// <summary>
        /// 清理当前界面管理的数据
        /// </summary>
        void Clear();

        /// <summary>
        /// 更新格子数据的New状态
        /// </summary>
        /// <param name="item">物品对象</param>
        void UpdateGridNewState(Item item);

        void DeleteItem(int itemId, int deleteNum);
        
        void DeleteItem(long persistentId);
        IEnumerable<Item> GetAllItems();
    }
}
