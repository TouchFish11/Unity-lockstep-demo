using System;
using System.Threading.Tasks;
using Core.DI;
using Core.Log;
using Core.UI;
using Core.UI.MVC;
using HotUpdate.Game.Data;
using HotUpdate.UI.Inventory;

namespace HotUpdate.UI.UI
{
    public class MainController : UIController<MainPanel, MainModel>
    {
        [Inject] private GameDataManager _dataManager;
        
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
                    _dataManager.ItemDataProvider.AddData(10001, 1);
                    _dataManager.ItemDataProvider.AddData(10002, 2);
                    _dataManager.ItemDataProvider.AddData(10003, 3);
                    _dataManager.ItemDataProvider.AddData(20001, 1);
                    _dataManager.ItemDataProvider.AddData(20002, 1);
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"{nameof(MainController)}: :{nameof(OnButtonClick)} error:{e.Message}");
            }
        }
    }
}
