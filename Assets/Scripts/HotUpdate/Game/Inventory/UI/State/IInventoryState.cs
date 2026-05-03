using System.Threading.Tasks;
using HotUpdate.Common.Items;

namespace HotUpdate.Game.Inventory.UI.State
{
    /// <summary>
    /// 背包界面状态接口
    /// </summary>
    public interface IInventoryState
    {
        Task Enter();

        Task OnItemClick(Item item);
        
        Task Exit();
        
        void OnItemsRefreshed();
    }
}
