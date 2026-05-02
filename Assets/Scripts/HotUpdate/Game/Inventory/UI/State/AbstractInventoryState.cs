using System;
using System.Threading.Tasks;
using Core.AssetBundles.Management;
using Core.DI;
using Core.Mono;
using Core.UI;
using Core.Utility;
using HotUpdate.Base.Inventory;
using HotUpdate.Common.Items;
using UnityEngine;
using Logger = Core.Log.Logger;

namespace HotUpdate.Game.Inventory.UI.State
{
    public abstract class AbstractInventoryState : IInventoryState
    {
        [Inject] protected IMonoAdapter monoAdapter;
        [Inject] protected IUIManager uiManager;
        [Inject] protected ObjectSpawner _objectSpawner;
        [Inject] protected IInventoryManager _inventoryManager;
        
        protected readonly InventoryController inventoryController;
        protected readonly InventoryPanel view;
        protected readonly InventoryModel model;
        
        protected AbstractInventoryState(InventoryModel model, InventoryPanel view, InventoryController inventoryController)
        {
            this.model = model;
            this.view = view;
            this.inventoryController = inventoryController;
        }
        
        public Task Enter()
        {
            inventoryController.OnButtonClickEvent += OnButtonClickEvent;
            inventoryController.OnScrollRectValueChangedEvent += OnScrollRectValueChangedEvent;
            return OnEnter();
        }
        
        /// <summary>
        /// 根据物品类型更新物品数据
        /// </summary>
        /// <param name="itemType"></param>
        protected async Task UpdateItemsByType(EItemType itemType)
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
            model.GridGenerator.AddClickListener(ItemClick);
            model.GridGenerator.SetDatas(items);
            // 手动更新一次，将协程转换为Task等待
            await TaskUtility.WaitForCoroutine(model.GridGenerator.FadeUpdateGrid(), monoAdapter);
            // 记录当前选择的物品类型
            model.CurrentItemType = itemType;
            // 显示第一个物品的详细信息
            if (items.Count > 0 && this is InventoryNormalState inventoryState)
            {
                await inventoryState.UpdateDetail(items[0]);
            }
        }

        protected async void ItemClick(Item item)
        {
            // 更新详细界面
            await UpdateDetail(item);
            // 更新New标志
            UpdateGridState(item);
            await OnItemClick(item);
        }

        protected abstract Task OnItemClick(Item item);
        
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
        protected void UpdateGridState(Item item)
        {
            // 是否点击了格子，移除new标识
            _inventoryManager.UpdateGridNewState(item);
        }

        private void OnButtonClickEvent(string btnName)
        {
            if (btnName == nameof(view.btnClose))
            {
                // 关闭背包界面
                uiManager.DestroyView(inventoryController.panelId);
                Logger.Log($"{nameof(InventoryController)}: {view.name} closed");
            }
        }
        
        private void OnScrollRectValueChangedEvent(string scrollViewName, Vector2 pos)
        {
            if (scrollViewName == nameof(view.svItems))
            {
                model.GridGenerator?.UpdateGrid();
            }
        }

        protected abstract Task OnEnter();

        public Task Exit()
        {
            inventoryController.OnButtonClickEvent -= OnButtonClickEvent;
            inventoryController.OnScrollRectValueChangedEvent -= OnScrollRectValueChangedEvent;
            return OnExit();
        }
        
        protected abstract Task OnExit();
    }
}
