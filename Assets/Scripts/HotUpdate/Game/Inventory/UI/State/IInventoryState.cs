using System.Threading.Tasks;

namespace HotUpdate.Game.Inventory.UI.State
{
    /// <summary>
    /// 背包界面状态接口
    /// </summary>
    public interface IInventoryState
    {
        Task Enter();
        
        Task Exit();
    }
}
