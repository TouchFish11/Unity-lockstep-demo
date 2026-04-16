using System.Threading.Tasks;
using Core.UI.MVC;

namespace HotUpdate.Game.Inventory.UI
{
    /// <summary>
    /// 背包界面控制器
    /// </summary>
    public class InventoryController : UIController<InventoryPanel, InventoryModel>
    {
        protected override Task OnShow()
        {
            throw new System.NotImplementedException();
        }

        protected override Task OnHide()
        {
            throw new System.NotImplementedException();
        }

        protected override Task OnInit()
        {
            throw new System.NotImplementedException();
        }

        protected override void ButtonOnClick(string btnName)
        {
            if (btnName == nameof(view.btnClose))
            {
                // 关闭背包界面
                uiManager.DestroyView(panelId);
            }
        }
    }
}
