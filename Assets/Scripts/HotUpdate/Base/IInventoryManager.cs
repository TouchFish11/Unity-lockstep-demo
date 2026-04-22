using System.Collections.Generic;
using System.Threading.Tasks;
using HotUpdate.Common.Config.Item;
using HotUpdate.Common.Data.Inventory;

namespace HotUpdate.Base
{
    public interface IInventoryManager
    {
        Task<ItemDTO> CreateItemDTO(int instanceId, ItemConfig itemConfig, ItemData itemData);
        
        IEnumerable<ItemData> GetItems();

        /// <summary>
        /// 添加物品数据
        /// </summary>
        /// <param name="id"></param>
        /// <param name="num"></param>
        void AddItemData(int id, int num);

        Task<List<ItemDTO>> CreateItemDTOsAsync(EItemType itemType);
        void InitItemConfigs();

        /// <summary>
        /// 获取物品配置
        /// </summary>
        /// <param name="itemId">物品ID</param>
        /// <returns>未找到返回null</returns>
        ItemConfig GetItemConfig(int itemId);
    }
}
