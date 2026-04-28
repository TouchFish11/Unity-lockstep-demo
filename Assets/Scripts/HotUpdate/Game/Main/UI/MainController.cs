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
        
        protected override Task OnActive()
        {
            return Task.CompletedTask;
        }

        protected override Task OnInactivate()
        {
            return Task.CompletedTask;
        }

        protected override Task OnInit()
        {
            return Task.CompletedTask;
        }

        protected override async void OnButtonClick(string btnName)
        {
            try
            {
                if (btnName == nameof(view.btnInventory))
                {
                    await uiManager.CreateViewAsync<InventoryPanel, InventoryModel, InventoryController>(AssetKeys.InventoryPanel, E_UILayer.Mid);
                }
                else if (btnName == nameof(view.btnAddItem))
                {
                    // Test
                    _inventoryManager.AddData(1, 1);
                    _inventoryManager.AddData(2, 2);
                    _inventoryManager.AddData(3, 3);
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"{nameof(MainController)}: :{nameof(OnButtonClick)} error:{e.Message}");
            }
        }
    }
}
