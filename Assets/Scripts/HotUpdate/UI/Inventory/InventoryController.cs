using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.AssetBundles.Management;
using Core.DI;
using Core.Mono;
using Core.UI.MVC;
using Core.Utility;
using HotUpdate.Base.Grid;
using HotUpdate.Base.Inventory;
using HotUpdate.Common.Items;
using HotUpdate.Game.Inventory;
using HotUpdate.UI.Inventory.State;
using UnityEngine;
using Logger = Core.Log.Logger;

namespace HotUpdate.UI.Inventory
{
    /// <summary>
    /// 背包界面控制器
    /// </summary>
    public class InventoryController : UIController<InventoryPanel, InventoryModel>
    {
        [Inject] protected IInventoryManager _inventoryManager;
        [Inject] protected ObjectSpawner _objectSpawner;
        [Inject] protected IMonoAdapter monoAdapter;
        
        private readonly Dictionary<Type, IInventoryState> _inventoryStates = new();
        
        // 当前背包界面所处的状态
        private IInventoryState _currentInventoryState;
        
        public event Action<string> OnButtonClickEvent;
        public event Action<string, string> OnInputFieldValueChangedEvent;
        public event Action<string, float> OnSliderValueChangedEvent;
        
        protected override async Task OnInit()
        {
            // 初始化背包界面所有状态
            _inventoryStates.Add(typeof(InventoryDeleteState), DIContainer.Create<InventoryDeleteState>(parameterValues: new object[]
            {
                model,
                view,
                this
            }));
            
            // 初始化工厂
            model.InitDetailPanelFactory();
            // 初始化格子生成器
            InitGridGenerator();
            // 创建选项
            await InitTypeOpt();
        }
        
        protected override Task OnActive()
        {
            _currentInventoryState = null; // 默认就是普通模式
            return Task.FromResult(Task.CompletedTask);
        }

        protected override async Task OnInactivate()
        {
            await TransitionTo(null);
            // 先回收，再销毁，否则对象池会无法清理
            model.ClearOpt();
            _objectSpawner.Dispose();
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
        /// 根据物品类型更新物品数据
        /// </summary>
        /// <param name="itemType"></param>
        public async void UpdateItemsByType(EItemType itemType)
        {
            try
            {
                // 先通知当前状态执行逻辑
                _currentInventoryState?.OnItemsRefreshed();
                // 根据当前数据创建，先 await，避免惯性滚动触发创建
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
                if (items.Count > 0)
                {
                    await UpdateDetail(items[0]);
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"[{nameof(InventoryController)}]: item update fail, {e.Message}");
            }
        }
        
        protected async void ItemClick(Item item)
        {
            try
            {
                // 更新New标志
                UpdateGridNewState(item);
                // 删除模式才执行
                if (_currentInventoryState != null)
                {
                    // 更新待删除状态
                    UpdateGridDeleteState(item);
                    await _currentInventoryState.OnItemClick(item);
                }
                
                // 更新详细界面
                await UpdateDetail(item);
            }
            catch (Exception e)
            {
                Logger.LogError($"[{nameof(InventoryController)}]: item update fail, {e.Message}");
            }
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
        private void UpdateGridNewState(Item item)
        {
            // 是否点击了格子，移除new标识
            _inventoryManager.UpdateGridNewState(item);
        }

        /// <summary>
        /// 更新格子的删除标志
        /// </summary>
        /// <param name="item"></param>
        private void UpdateGridDeleteState(Item item)
        {
            // 更新格子的删除状态标志
            item.isDeleted = !item.isDeleted;
        }

        /// <summary>
        /// 过渡到指定状态
        /// </summary>
        /// <param name="nextState"></param>
        public async Task TransitionTo(Type nextState)
        {
            if(_currentInventoryState != null)
                await _currentInventoryState.Exit();

            if (nextState == null)
            {
                _currentInventoryState = null;
            }
            else
            {
                _currentInventoryState = _inventoryStates[nextState];
                await _currentInventoryState.Enter();
            }
        }
        
        /// <summary>
        /// 切换排序
        /// </summary>
        /// <param name="itemSort"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void SwitchSort(EItemSort itemSort)
        {
            model.sortComparison = itemSort switch
            {
                EItemSort.Default => InventorySorterFactory.DefaultIDSorter(1),
                EItemSort.Quality => InventorySorterFactory.QualitySorter(-1),
                _ => throw new ArgumentOutOfRangeException(nameof(itemSort), itemSort, null)
            };
            UpdateItemsByType(model.CurrentItemType);
        }
        
        /// <summary>
        /// 删除物品
        /// </summary>
        /// <param name="deletedItems"></param>
        public void ExecuteDelete(Dictionary<Item, int> deletedItems)
        {
            foreach (var (item, num) in deletedItems)
            {
                if (item.itemConfig.isPile)
                    _inventoryManager.DeleteItem(item.itemConfig.itemId, num);
                else
                    _inventoryManager.DeleteItem(item.persistentId);
            }
        }

        private Task EnterDeleteState()
        {
            SetGridVisualState(EGridState.Delete);
            // 切换为删除状态
            return TransitionTo(typeof(InventoryDeleteState));
        }

        public Task ExitDeleteState()
        {
            SetGridVisualState(EGridState.Normal);
            // 切换为正常状态
            return TransitionTo(null);
        }
        
        /// <summary>
        /// 设置格子的可视状态
        /// </summary>
        /// <param name="state"></param>
        public void SetGridVisualState(EGridState state)
        {
            // 重置Item删除标记和状态
            foreach (var item in _inventoryManager.GetAllItems())
            {
                item.isDeleted = false;
            }

            // 应用状态
            foreach (var itemCell in model.GridGenerator.GetAllCell())
            {
                itemCell.ApplyState(state);
            }
        }
        
        protected override async void OnButtonClick(string btnName)
        {
            if (btnName == nameof(view.btnClose))
            {
                await ExitDeleteState();
                // 关闭背包界面
                await uiManager.DestroyView(panelId);
                Logger.Log($"[{nameof(InventoryController)}]: {view.name} closed");
            }
            else if (btnName == nameof(view.btnRequestDelete))
            {
                await EnterDeleteState();
            }
            else
            {
                OnButtonClickEvent?.Invoke(btnName);
            }
        }
        
        protected override void OnScrollRectValueChanged(string scrollViewName, Vector2 pos)
        {
            if (scrollViewName == nameof(view.svItems))
            {
                model.GridGenerator?.UpdateGrid();
            }
        }
        
        protected override void OnDropdownValueChanged(string dropdownName, int index)
        {
            try
            {
                if (dropdownName != nameof(view.dpSorts))
                    return;
                
                switch (index)
                {
                    case 0:
                        SwitchSort(EItemSort.Default);
                        break;
                    case 1:
                        SwitchSort(EItemSort.Quality);
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

        protected override void OnInputFieldValueChanged(string fieldName, string inputStr)
        {
            OnInputFieldValueChangedEvent?.Invoke(fieldName, inputStr);
        }
        
        protected override void OnSliderValueChanged(string sliderName, float value)
        {
            OnSliderValueChangedEvent?.Invoke(sliderName, value);
        }

        protected override Task OnDestroy()
        {
            _inventoryStates.Clear();
            _currentInventoryState = null;
            return Task.CompletedTask;
        }
    }
}
