using System.Collections.Generic;
using System.Threading.Tasks;
using HotUpdate.Common.Config.Item;
using HotUpdate.Common.Data.Inventory;

namespace HotUpdate.Base.Inventory
{
    public interface IInventoryManager
    {
        /// <summary>
        /// 获取所有物品对象
        /// </summary>
        /// <returns></returns>
        IEnumerable<ItemData> GetItems();

        /// <summary>
        /// 添加物品数据
        /// </summary>
        /// <param name="id"></param>
        /// <param name="num"></param>
        void AddData(int id, int num);

        /// <summary>
        /// 异步创建物品对象
        /// </summary>
        /// <param name="itemType"></param>
        /// <returns></returns>
        Task<List<Item>> CreateItemsAsync(EItemType itemType);

        /// <summary>
        /// 通过物品实例ID获取物品数据
        /// </summary>
        /// <param name="instanceId"></param>
        /// <returns></returns>
        ItemData GetData(int instanceId);

        /// <summary>
        /// 删除物品数据
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="num"></param>
        void DeleteData(int itemId, int num);

        /// <summary>
        /// 清理当前界面管理的数据
        /// </summary>
        void Clear();
    }
}
