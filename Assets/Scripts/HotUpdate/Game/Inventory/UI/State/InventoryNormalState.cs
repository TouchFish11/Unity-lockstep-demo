using System;
using System.Threading.Tasks;
using Core.AssetBundles.Management;
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
            inventoryController.OnButtonClickEvent += OnOnButtonClickEvent;
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
        
        protected override async Task OnItemClick(Item item)
        {
            await UpdateDetail(item);
            UpdateGridState(item);
        }
        
        /// <summary>
        /// 更新详细界面
        /// </summary>
        /// <param name="item"></param>
        /// <exception cref="Exception"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public async Task UpdateDetail(Item item)
        {
            PoolObject poolObject = default;
            try
            {
                var itemConfig = item.itemConfig;
                var itemData = _inventoryManager.GetData(item);

                if (!model.DetailPanelPoolObject.Obj || model.CurrentItemType != itemConfig.itemType)
                {
                    if(model.DetailPanelPoolObject.Obj)
                        model.DetailPanelPoolObject.Collect();
                    // 工厂创建详细界面
                    poolObject = await model.DetailPanelFactory.CreateDetailPanel(itemConfig.itemType, view.DetailArea);
                    model.DetailPanelPoolObject = poolObject.Convert<InventoryDetailPanel>();
                }

                // 初始化详细界面
                model.DetailPanelPoolObject.Obj.UpdateInfo(itemConfig, itemData);
            }
            catch (Exception e)
            {
                poolObject.Collect(true);
                Logger.LogError($"{nameof(InventoryController)}: Create detail panel fail, {e.Message}");
            }
        }
        
        /// <summary>
        /// 更新格子状态
        /// </summary>
        /// <param name="item"></param>
        public void UpdateGridState(Item item)
        {
            // 是否点击了格子，移除new标识
            _inventoryManager.UpdateGridNewState(item);
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

        private async void OnOnButtonClickEvent(string btnName)
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
            inventoryController.OnButtonClickEvent -= OnOnButtonClickEvent;
            return Task.CompletedTask;
        }
    }
}
