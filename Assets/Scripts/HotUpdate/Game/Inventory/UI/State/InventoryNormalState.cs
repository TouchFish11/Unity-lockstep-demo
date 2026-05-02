using System;
using System.Threading.Tasks;
using HotUpdate.Base.Grid;
using HotUpdate.Common.Items;
using Logger = Core.Log.Logger;

namespace HotUpdate.Game.Inventory.UI.State
{
    /// <summary>
    /// 背包界面正常业务状态
    /// </summary>
    public class InventoryNormalState : AbstractInventoryState
    {
        public InventoryNormalState(InventoryModel model, InventoryPanel view, InventoryController inventoryController) : 
            base(model, view, inventoryController)
        {
            
        }
        
        protected override Task OnEnter()
        {
            inventoryController.OnDropdownValueChangedEvent += OnDropdownValueChangedEvent;
            inventoryController.OnButtonClickEvent += OnButtonClickEvent;
            foreach (var itemCell in model.GridGenerator.GetAllCell())
            {
                itemCell.SwitchAction(ItemCell.EGridAction.Normal);
                itemCell.SetClick(ItemClick);
            }
            return Task.CompletedTask;
        }
        
        public void InitGridGenerator()
        {
            // 创建格子生成器
            var builder = GridGeneratorBuilder<Item, ItemCell>.Create();
            var generator = builder.CreateGenerator(EGridLayout.Vertical)
                .SetParent(view.svItems)
                .SetOriginOffset(15, -15)
                .SetGridSize(100, 100)
                .SetGridSpace(15, 15)
                .SetColumn(8)
                .Build();
            
            // 保存生成器
            model.GridGenerator = generator;
        }
        
        /// <summary>
        /// 初始化侧面选项
        /// </summary>
        public async Task InitTypeOpt()
        {
            view.OptGroup.allowSwitchOff = true;
            foreach (var itemType in Enum.GetValues(typeof(EItemType)))
            {
                var opt = await _objectSpawner.SpawnAsync<ItemTypeOpt>(AssetKeys.ItemTypeOpt, view.svOpts.content);
                opt.Obj.InitOption((EItemType)itemType, null, view.OptGroup);
                opt.Obj.OnItemTypeOptChange += UpdateItemsByType;
                model.AddItemTypeOpt(opt);
            }
            // // 默认选择第一个选项
            model.GetFirstItemTypeOpt().Select();
            view.OptGroup.allowSwitchOff = false;
        }
        
        /// <summary>
        /// 切换排序
        /// </summary>
        /// <param name="itemSort"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private Task SwitchSort(EItemSort itemSort)
        {
            model.sortComparison = itemSort switch
            {
                EItemSort.Default => InventorySorterFactory.DefaultIDSorter(1),
                EItemSort.Quality => InventorySorterFactory.QualitySorter(-1),
                _ => throw new ArgumentOutOfRangeException(nameof(itemSort), itemSort, null)
            };
            return UpdateItemsByType(model.CurrentItemType);
        }

        protected override Task OnItemClick(Item item)
        {
            return Task.CompletedTask;
        }

        private async void OnButtonClickEvent(string btnName)
        {
            try
            {
                if (btnName == nameof(view.btnRequestDelete))
                {
                    // 切换为删除状态
                    await inventoryController.TransitionTo(typeof(InventoryDeleteState));
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"[{nameof(InventoryController)}]: {e.Message}");
            }
        }

        private async void OnDropdownValueChangedEvent(string dropdownName, int index)
        {
            try
            {
                if (dropdownName != nameof(view.dpSorts))
                    return;
                
                switch (index)
                {
                    case 0:
                        await SwitchSort(EItemSort.Default);
                        break;
                    case 1:
                        await SwitchSort(EItemSort.Quality);
                        break;
                }
            }
            catch (OperationCanceledException canceledException)
            {
                Logger.Log($"[{nameof(InventoryController)}]: operator cancel, {canceledException.Message}");
            }
            catch (Exception e)
            {
                Logger.LogError($"[{nameof(InventoryController)}]: {e.Message}");
            }
        }

        protected override Task OnExit()
        {
            inventoryController.OnDropdownValueChangedEvent -= OnDropdownValueChangedEvent;
            inventoryController.OnButtonClickEvent -= OnButtonClickEvent;
            return Task.CompletedTask;
        }
    }
}
