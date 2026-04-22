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
                var opt = await _objectSpawner.SpawnAsync<ItemTypeOpt>(AssetKeys.Itemtypeopt, view.svOpts.content);
                opt.Obj.InitOption((EItemType)itemType, null, view.OptGroup);
                opt.Obj.OnItemTypeOptChange += UpdateItemsByType;
                model.AddItemTypeOpt(opt);
            }
            // // 默认选择第一个选项
            model.GetFirstItemTypeOpt().Select();
            view.OptGroup.allowSwitchOff = false;
        }
        
        /// <summary>
        /// 根据物品类型更新物品数据
        /// </summary>
        /// <param name="itemType"></param>
        private async Task UpdateItemsByType(EItemType itemType)
        {
            // 清空上次显示的格子
            model.GridGenerator?.ClearGrids();
            // 根据当前数据创建DTO
            var itemDTOs = await _inventoryManager.CreateItemDTOsAsync(itemType);
            // 排序物品数据DTO，默认按照品质类型排序    
            itemDTOs.Sort((x, y) => x.qualityType.CompareTo(y.qualityType)); // TODO:可以抽象排序行为，提供按钮让玩家选择如何排序

            using var handle = await GameAsset.LoadAssetAsync<GameObject>(AssetKeys.Itemcell);
            var rectTransform = handle.Asset.GetComponent<RectTransform>();
            
            // 创建格子生成器
            var builder = GridGeneratorBuilder<ItemDTO, ItemCell>.Create();
            var generator = builder.CreateGenerator(EGridLayout.Vertical)
                .SetParent(view.svItems)
                .SetGridSize(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y)
                .SetGridSpace(15, 15)
                .SetDatas(itemDTOs)
                .SetColumn(8)
                .SetClick(UpdateDetail)
                .SetSelectIndex(0)
                .Build();
            
            // 立即重建所有 Canvas 布局
            Canvas.ForceUpdateCanvases();   // TODO：暂时这样处理，可能是因为UI还没有重建，可能需要等待
            // 手动更新一次
            generator.UpdateGrid();
            // 保存生成器
            model.GridGenerator = generator;
        }

        /// <summary>
        /// 更新详细界面
        /// </summary>
        /// <param name="itemDTO"></param>
        /// <exception cref="Exception"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private async void UpdateDetail(ItemDTO itemDTO)
        {
            var itemConfig = _inventoryManager.GetItemConfig(itemDTO.itemId);
            var itemData = _inventoryManager.GetData(itemDTO);
            if(itemConfig == null)
                throw new Exception($"ItemConfig {itemDTO.itemId} not found");
            
            switch (itemDTO.itemType)
            {
                case EItemType.Material:
                    if (model.InventoryDetailPanel == null)
                    {
                        var poolObject = await _objectSpawner.SpawnAsync<MaterialDetailPanel>(AssetKeys.Materialdetailpanel, view.DetailArea);
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

        // 按钮点击事件回调
        protected override void ButtonOnClick(string btnName)
        {
            if (btnName == nameof(view.btnClose))
            {
                // 关闭背包界面
                uiManager.DestroyView(panelId);
            }
        }

        protected override void ScrollRectValueChanged(string scrollViewName, Vector2 pos)
        {
            if (scrollViewName == nameof(view.svItems))
            {
                model.GridGenerator.UpdateGrid();
            }
        }
    }
}
