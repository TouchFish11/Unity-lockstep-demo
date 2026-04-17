using System;
using System.Threading.Tasks;
using Core.Log;
using Core.UI;
using Core.UI.MVC;
using HotUpdate.Game.Inventory.UI;

namespace HotUpdate.Game.Main.UI
{
    public class MainController : UIController<MainPanel, MainModel>
    {
        protected override Task OnShow()
        {
            return Task.CompletedTask;
        }

        protected override Task OnHide()
        {
            return Task.CompletedTask;
        }

        protected override Task OnInit()
        {
            return Task.CompletedTask;
        }

        protected override async void ButtonOnClick(string btnName)
        {
            try
            {
                if (btnName == nameof(view.btnInventory))
                {
                    await uiManager.CreateViewAsync<InventoryPanel, InventoryModel, InventoryController>("", E_UILayer.Mid);
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"{nameof(MainController)}: :{nameof(ButtonOnClick)} error:{e.Message}");
            }
        }
    }
}
