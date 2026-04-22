using System;
using System.Threading.Tasks;
using Core.DI;
using Core.Log;
using Core.UI;
using Core.UI.MVC;
using HotUpdate.Base;
using HotUpdate.Game.Inventory.UI;

namespace HotUpdate.Game.Main.UI
{
    public class MainController : UIController<MainPanel, MainModel>
    {
        [Inject] private IInventoryManager _inventoryManager;
        
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
                    await uiManager.CreateViewAsync<InventoryPanel, InventoryModel, InventoryController>(AssetKeys.Inventorypanel, E_UILayer.Mid);
                }
                else if (btnName == nameof(view.btnAddItem))
                {
                    // Test
                    _inventoryManager.AddItemData(1, 1);
                    _inventoryManager.AddItemData(2, 2);
                    _inventoryManager.AddItemData(3, 3);
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"{nameof(MainController)}: :{nameof(ButtonOnClick)} error:{e.Message}");
            }
        }
    }
}
