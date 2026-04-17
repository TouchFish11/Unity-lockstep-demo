using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.AssetBundles.Management;
using Core.DI;
using Core.UI.MVC;
using HotUpdate.Common.Config.Item;
using HotUpdate.Common.Data.Inventory;
using HotUpdate.Game.Inventory.UI.Detail;

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

        private async Task InitTypeOpt()
        {
            view.OptGroup.allowSwitchOff = true;
            foreach (var itemType in Enum.GetValues(typeof(EItemType)))
            {
                var opt = await _objectSpawner.SpawnAsync<ItemTypeOpt>("", view.svOpts.content);
                opt.Obj.InitOption((EItemType)itemType, null, view.OptGroup);
                opt.Obj.OnItemTypeOptChange += UpdateItemsByType;
            }

            // 默认选择第一个选项
            model.GetFirstItemTypeOpt().Select();
            view.OptGroup.allowSwitchOff = false;
        }
        
        private async Task UpdateItemsByType(EItemType itemType)
        {
            // 清空上次显示的格子
            model.ClearItems();
            // 根据当前分类筛选数据
            var itemDatas = _inventoryManager.GetItemDataByType(itemType);
            // 根据当前玩家物品数量创建物品格子
            var itemCells = await CreateItemCells(itemDatas.Count);
            // 根据当前数据创建DTO
            var itemDTOs = await CreateItemDTOs(itemDatas);
            // 排序物品数据DTO，默认按照品质类型排序    TODO:可以抽象排序行为，提供按钮让玩家选择如何排序
            itemDTOs.Sort((x, y) => x.qualityType.CompareTo(y.qualityType));
            // 初始化格子
            for (var i = 0; i < itemCells.Length; i++)
            {
                var itemCell = itemCells[i];
                itemCell.Obj.InitItem(itemDTOs[i]);
                itemCell.Obj.OnSelect += UpdateDetail;
                // 缓存格子
                model.AddItem(itemCell);
            }
            
            // 默认选中第一个格子
            model.GetFirstItem().Select();
        }

        private async Task UpdateDetail(ItemDTO itemDTO)
        {
            var itemConfig = _inventoryManager.ItemConfigCollection.itemConfigs.Find(config => config.itemId == itemDTO.itemId);
            if(itemConfig == null)
                throw new Exception($"ItemConfig {itemDTO.itemId} not found");
            
            switch (itemDTO.itemType)
            {
                case EItemType.Material:
                    var materialDetailPanel = await _objectSpawner.SpawnAsync<MaterialDetailPanel>("Material", view.DetailArea);
                    materialDetailPanel.Obj.UpdateInfo(itemConfig.name, itemConfig.description);
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
        
        private Task<PoolObject<ItemCell>[]> CreateItemCells(int itemCellCount)
        {
            var itemCellTasks = new List<Task<PoolObject<ItemCell>>>();
            for (var i = 0; i < itemCellCount; i++)
            {
                itemCellTasks.Add(_objectSpawner.SpawnAsync<ItemCell>("", view.svItems.content));
            }
            
            // 等待所有格子创建完成
            return Task.WhenAll(itemCellTasks);
        }

        private async Task<List<ItemDTO>> CreateItemDTOs(List<ItemData> itemDatas)
        {
            // 创建所有DTO对象
            var dtoTasks = new List<Task<ItemDTO>>();
            foreach (var itemData in itemDatas)
            {
                var itemConfig = _inventoryManager.ItemConfigCollection.itemConfigs.Find(config => config.itemId == itemData.itemId);
                dtoTasks.Add(_inventoryManager.CreateItemDTO(itemConfig, itemData));
            }
            
            // 等待所有DTO对象创建完成
            return new List<ItemDTO>(await Task.WhenAll(dtoTasks));
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
