using System;
using System.Threading.Tasks;
using Core.AssetBundles.Management;
using Core.DI;
using Core.UI.MVC;
using HotUpdate.Base.Factory;
using HotUpdate.Base.Grid;
using HotUpdate.Common.Config.Item;
using HotUpdate.Game.Inventory.UI.Detail;
using UnityEngine;
using Logger = Core.Log.Logger;

namespace HotUpdate.Game.Inventory.UI
{
    /// <summary>
    /// 背包界面控制器
    /// </summary>
    public class InventoryController : UIController<InventoryPanel, InventoryModel>
    {
        [Inject] private InventoryManager _inventoryManager;
        [Inject] private ObjectSpawner _objectSpawner;
        
        protected override Task OnInit()
        {
            return Task.CompletedTask;
        }
        
        protected override async Task OnShow()
        {
            // 初始化格子生成器
            await InitGridGenerator();
            // 创建选项
            await InitTypeOpt();
        }

        protected override Task OnHide()
        {
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

        private async Task InitGridGenerator()
        {
            using var handle = await GameAsset.LoadAssetAsync<GameObject>(AssetKeys.ItemCell);
            var rectTransform = handle.Asset.GetComponent<RectTransform>();
            
            // 创建格子生成器
            var builder = GridGeneratorBuilder<ItemDTO, ItemCell>.Create();
            var generator = builder.CreateGenerator(EGridLayout.Vertical)
                .SetParent(view.svItems)
                .SetGridSize(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y)
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
            // 清空上次显示的格子
            model.GridGenerator.ClearGrids();
            // 根据当前数据创建DTO
            var itemDTOs = await _inventoryManager.CreateItemDTOsAsync(itemType);
            // 排序物品数据DTO，默认按照品质类型排序    
            itemDTOs.Sort(model.sortComparison);
            // 初始化生成器
            model.GridGenerator.SetClick(UpdateDetail);
            model.GridGenerator.SetSelectIndex(0);
            model.GridGenerator.SetDatas(itemDTOs);
            // 手动更新一次
            model.GridGenerator.FadeUpdateGrid();
            // 记录当前选择的物品类型
            model.CurrentItemType = itemType;
        }

        /// <summary>
        /// 更新详细界面
        /// </summary>
        /// <param name="itemDTO"></param>
        /// <exception cref="Exception"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private async void UpdateDetail(ItemDTO itemDTO)
        {
            try
            {
                var itemConfig = _inventoryManager.GetItemConfig(itemDTO.itemId);
                var itemData = _inventoryManager.GetData(itemDTO);
            
                switch (itemDTO.itemType)
                {
                    case EItemType.Material:
                        if (model.InventoryDetailPanel == null)
                        {
                            var poolObject = await _objectSpawner.SpawnAsync<MaterialDetailPanel>(AssetKeys.MaterialDetailPanel, view.DetailArea);
                            poolObject.Obj.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                            model.InventoryDetailPanel = poolObject.Obj;
                            model.DetailPanelPoolObject = poolObject;
                        }
                        model.InventoryDetailPanel.UpdateInfo(itemConfig, itemData);
                        break;
                    case EItemType.Weapon:
                        await _objectSpawner.SpawnAsync<MaterialDetailPanel>("Weapon", view.DetailArea);
                        break;
                    case EItemType.HolyRelic:
                        await _objectSpawner.SpawnAsync<MaterialDetailPanel>("HolyRelic", view.DetailArea);
                        break;
                    case EItemType.precious:
                        await _objectSpawner.SpawnAsync<MaterialDetailPanel>("precious", view.DetailArea);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"{nameof(InventoryController)}.{nameof(UpdateDetail)}: {e.Message}");
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
        protected override void ButtonOnClick(string btnName)
        {
            if (btnName == nameof(view.btnClose))
            {
                // 关闭背包界面
                uiManager.DestroyView(panelId);
                // 清理资源缓存
                
            }
        }

        protected override void ScrollRectValueChanged(string scrollViewName, Vector2 pos)
        {
            if (scrollViewName == nameof(view.svItems))
            {
                model.GridGenerator.UpdateGrid();
            }
        }

        protected override async void DropdownValueChanged(string dropdownName, int index)
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
            catch (Exception e)
            {
                Logger.LogError($"{nameof(InventoryController)}.{nameof(DropdownValueChanged)}: {e.Message}");
            }
        }
    }
}
