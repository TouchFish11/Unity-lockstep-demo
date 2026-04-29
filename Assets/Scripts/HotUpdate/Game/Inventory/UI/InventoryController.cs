using System;
using System.Threading.Tasks;
using Core.AssetBundles.Management;
using Core.DI;
using Core.UI.MVC;
using HotUpdate.Base.Grid;
using HotUpdate.Base.Inventory;
using HotUpdate.Common.Config.Item;
using UnityEngine;
using Logger = Core.Log.Logger;

namespace HotUpdate.Game.Inventory.UI
{
    /// <summary>
    /// 背包界面控制器
    /// </summary>
    public class InventoryController : UIController<InventoryPanel, InventoryModel>
    {
        [Inject] private IInventoryManager _inventoryManager;
        [Inject] private ObjectSpawner _objectSpawner;  // 生成器不在这里写
        
        protected override Task OnInit()
        {
            return Task.CompletedTask;
        }
        
        protected override async Task OnActive()
        {
            // 初始化工厂
            model.InitDetailPanelFactory();
            // 初始化格子生成器
            InitGridGenerator();
            // 创建选项
            await InitTypeOpt();
        }

        protected override Task OnInactivate()
        {
            // 先回收，再销毁，否则对象池会无法清理
            model.ClearOpt();
            _objectSpawner.Dispose();
            _objectSpawner = null;
            return Task.CompletedTask;
        }

        /// <summary>
        /// 初始化侧面选项
        /// </summary>
        private async Task InitTypeOpt()
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

        private void InitGridGenerator()
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
        /// 根据物品类型更新物品数据
        /// </summary>
        /// <param name="itemType"></param>
        private async Task UpdateItemsByType(EItemType itemType)
        {
            // 根据当前数据创建DTO，先 await，避免惯性滚动触发创建
            var items = await _inventoryManager.CreateItemsAsync(itemType);
            /*
             * 清空上次显示的格子，清空格子一定要在await后执行，因为当调用UpdateItemsByType方法时，若先清空，但是界面可能还在因为惯性在滚动
             * 导致又触发ScrollRectValueChanged事件创建新的格子，导致清空后又有格子，会出现显示异常和重复创建格子的问题
             * 因为这个异步方法会被挂起的副作用，所以等待await后再执行清理格子，就能避免这个问题
            */
            model.GridGenerator.ClearGrids();
            // 排序物品数据DTO，默认按照品质类型排序    
            items.Sort(model.sortComparison);
            // 初始化生成器
            model.GridGenerator.SetClick(UpdateDetail);
            model.GridGenerator.SetSelectIndex(0);
            model.GridGenerator.SetDatas(items);
            // 手动更新一次
            model.GridGenerator.FadeUpdateGrid();
            // 记录当前选择的物品类型
            model.CurrentItemType = itemType;
        }

        /// <summary>
        /// 更新详细界面
        /// </summary>
        /// <param name="item"></param>
        /// <exception cref="Exception"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private async void UpdateDetail(Item item)
        {
            PoolObject poolObject = default;
            try
            {
                var itemConfig = item.itemConfig;
                var itemData = _inventoryManager.GetData(item.instanceId);

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
        
        // 按钮点击事件回调
        protected override void OnButtonClick(string btnName)
        {
            if (btnName == nameof(view.btnClose))
            {
                // 关闭背包界面
                uiManager.DestroyView(panelId);
                Logger.Log($"{nameof(InventoryController)}: {view.name} closed");
            }
        }

        protected override void OnScrollRectValueChanged(string scrollViewName, Vector2 pos)
        {
            if (scrollViewName == nameof(view.svItems))
            {
                model.GridGenerator?.UpdateGrid();
            }
        }

        protected override async void OnDropdownValueChanged(string dropdownName, int index)
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
    }
}
