using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.DI;
using Core.UI;
using HotUpdate.Base.Inventory;
using HotUpdate.Base.Tip;
using HotUpdate.Common.Items;
using HotUpdate.UI.Inventory.ViewModel;
using HotUpdate.UI.Tip;
using Logger = Core.Log.Logger;

namespace HotUpdate.UI.Inventory.State
{
    /// <summary>
    /// 背包界面删除状态
    /// </summary>
    public class InventoryDeleteState : IInventoryState
    {
        [Inject] private IInventoryManager _inventoryManager;
        [Inject] private IUIManager _uiManager;
        
        private readonly InventoryController _inventoryController;
        private readonly InventoryDeleteViewModel _inventoryDeleteViewModel;
        
        private readonly Dictionary<Item, int> _deletedItems = new();
        
        // 当前正在操作的物品对象
        private Item _currentItem;
        /// 默认最小删除数量
        private const int DefaultDeleteMinNum = 1;
        
        public InventoryDeleteState(InventoryDeleteViewModel viewModel, InventoryController inventoryController)
        {
            _inventoryDeleteViewModel = viewModel;
            _inventoryController = inventoryController;
        }
        
        public Task Enter()
        {
            // 监听添加减少按钮点击
            _inventoryController.OnButtonClickEvent += OnButtonClickEvent;
            // 监听输入框事件
            _inventoryController.OnInputFieldValueChangedEvent += OnInputFieldValueChangedEvent;
            // 监听滑动条事件
            _inventoryController.OnSliderValueChangedEvent += OnSliderValueChanged;
            // 激活删除区域
            _inventoryDeleteViewModel.DeleteAreaActive.Value = true;
            // 隐藏删除盒，因为还没有选择删除的物品
            _inventoryDeleteViewModel.DeleteBoxActive.Value = false;
            return Task.CompletedTask;
        }

        public Task OnItemClick(Item item)
        {
            // 先记录当前操作的物品
            _currentItem = item;
            // 更新滑动条的最最大/最小值
            var itemData = _inventoryManager.GetData(_currentItem);
            _inventoryDeleteViewModel.DeleteSliderExtremum.Value = (DefaultDeleteMinNum, itemData.itemNum);

            // 点击相同格子移除销毁状态
            if (_deletedItems.Remove(item))
            {
                // 隐藏删除盒的输入
                _inventoryDeleteViewModel.DeleteBoxActive.Value = false;
            }
            // 新增销毁物品
            else
            {
                _deletedItems.Add(item, DefaultDeleteMinNum);
                if (item.itemConfig.isPile)
                {
                    // 激活删除盒的输入
                    _inventoryDeleteViewModel.DeleteBoxActive.Value = true;
                    UpdateDeleteStateUI(DefaultDeleteMinNum);
                }
                else
                {
                    // 隐藏删除盒的输入
                    _inventoryDeleteViewModel.DeleteBoxActive.Value = false;
                }
            }
            return Task.CompletedTask;
        }

        private async void OnButtonClickEvent(string btnName)
        {
            if (btnName == "btnAdd")
            {
                // 能进入这里的逻辑，一定是可堆叠的物品
                var currentNum = _deletedItems[_currentItem];
                ++currentNum;
                // 同步字典缓存
                _deletedItems[_currentItem] = currentNum;
                UpdateDeleteStateUI(currentNum);
            }
            else if (btnName == "btnSub")
            {
                // 能进入这里的逻辑，一定是可堆叠的物品
                var currentNum = _deletedItems[_currentItem];
                --currentNum;
                // 同步字典缓存
                _deletedItems[_currentItem] = currentNum;
                UpdateDeleteStateUI(currentNum);
            }
            else if (btnName == "btnDelete")
            {
                await RequestDelete();
            }
            else if (btnName == "btnCancelDelete")
            {
                // 删除完毕后，退出删除状态
                await _inventoryController.ExitDeleteState();
            }
            else if (btnName == "btnMin")
            {
                _deletedItems[_currentItem] = DefaultDeleteMinNum;
                UpdateDeleteStateUI(DefaultDeleteMinNum);
            }
            else if (btnName == "btnMax")
            {
                var itemData = _inventoryManager.GetData(_currentItem);
                _deletedItems[_currentItem] = itemData.itemNum;
                UpdateDeleteStateUI(itemData.itemNum);
            }
        }

        /// <summary>
        /// 请求删除物品
        /// </summary>
        private async Task RequestDelete()
        {
            // 打开删除确认界面
            var tipController = await _uiManager.CreateViewAsync<TipView, TipController>(AssetKeys.TipPanel, E_UILayer.Mid);
            // 初始化确认数据
            var confirmData = DIContainer.Create<ConfirmData>();
            confirmData.ConfirmTitle = "删除提示";
            confirmData.ConfirmContent = EConfirmContent.Delete;
            confirmData.ContentData = new Dictionary<Item,int>(_deletedItems);
            confirmData.ConfirmMessage = "以下物品将被销毁";
            confirmData.OnConfirm = ExecuteDelete;
            // 设置提示界面
            tipController.SetTip(confirmData);
        }

        /// <summary>
        /// 执行删除
        /// </summary>
        private async void ExecuteDelete()
        {
            try
            {
                // 删除物品
                _inventoryController.ExecuteDelete(_deletedItems);
                // 刷新当前界面
                _inventoryController.UpdateItemsByType(_inventoryController.CurrentItemType);
                // 删除完毕后，退出删除状态
                await _inventoryController.ExitDeleteState();
            }
            catch (Exception e)
            {
                Logger.LogError($"{e.Message}");
            }
        }
        
        /// <summary>
        /// 更新所有删除相关的UI显示
        /// </summary>
        /// <param name="currentNum"></param>
        private void UpdateDeleteStateUI(int currentNum)
        {
            // 可堆叠物品
            if (!_currentItem.itemConfig.isPile) 
                return;
            
            // 获取玩家物品拥有数量
            var itemData = _inventoryManager.GetData(_currentItem);
            if (currentNum == DefaultDeleteMinNum && itemData.itemNum == DefaultDeleteMinNum)
            {
                _inventoryDeleteViewModel.AddDeleteBtnEnalbe.Value = false;
                _inventoryDeleteViewModel.SubDeleteBtnEnalbe.Value = false;
                _inventoryDeleteViewModel.MaxDeleteBtnEnalbe.Value = false;
                _inventoryDeleteViewModel.MinDeleteBtnEnalbe.Value = false;
                
                _inventoryDeleteViewModel.InputFieldDeleteNum.Value = DefaultDeleteMinNum.ToString();
                _inventoryDeleteViewModel.DeleteSliderNum.Value = DefaultDeleteMinNum;
                return;
            }
                
            // 删除数量不允许超过拥有数量
            if (currentNum == itemData.itemNum)
            {
                // 禁用增加/最大按钮
                _inventoryDeleteViewModel.AddDeleteBtnEnalbe.Value = false;
                _inventoryDeleteViewModel.MaxDeleteBtnEnalbe.Value = false;
                // 启用最小/减少按钮
                _inventoryDeleteViewModel.SubDeleteBtnEnalbe.Value = true;
                _inventoryDeleteViewModel.MinDeleteBtnEnalbe.Value = true;
            }
            // 删除数量不允许为负数或0
            else if(currentNum == 1)
            {
                // 禁用最小/减少按钮
                _inventoryDeleteViewModel.SubDeleteBtnEnalbe.Value = false;
                _inventoryDeleteViewModel.MinDeleteBtnEnalbe.Value = false;
                    
                // 启用增加/最大按钮
                _inventoryDeleteViewModel.AddDeleteBtnEnalbe.Value = true;
                _inventoryDeleteViewModel.MaxDeleteBtnEnalbe.Value = true;
            }
            else
            {
                _inventoryDeleteViewModel.AddDeleteBtnEnalbe.Value = true;
                _inventoryDeleteViewModel.SubDeleteBtnEnalbe.Value = true;
                _inventoryDeleteViewModel.MaxDeleteBtnEnalbe.Value = true;
                _inventoryDeleteViewModel.MinDeleteBtnEnalbe.Value = true;
            }
                
            // 更新输入框显示
            if(_inventoryDeleteViewModel.InputFieldDeleteNum.Value != currentNum.ToString())
                _inventoryDeleteViewModel.InputFieldDeleteNum.Value = currentNum.ToString();
            // 更新滑动条显示
            if((int)_inventoryDeleteViewModel.DeleteSliderNum.Value != currentNum)
                _inventoryDeleteViewModel.DeleteSliderNum.Value = currentNum;
        }
        
        private void OnInputFieldValueChangedEvent(string inputFieldName, string inputFieldValue)
        {
            if (inputFieldName == "inputFieldDeleteNum")
            {
                if(_currentItem == null)
                    return;
                
                // 能进入这里的逻辑，一定是可堆叠的物品
                // 转换成功才处理
                if (int.TryParse(inputFieldValue, out var currentNum))
                {
                    if (_deletedItems.ContainsKey(_currentItem))
                        _deletedItems[_currentItem] = currentNum;
                    UpdateDeleteStateUI(currentNum);
                }
                else
                {
                    Logger.Log($"Invalid input: {inputFieldValue}");
                }
            }
        }

        private void OnSliderValueChanged(string sliderName, float value)
        {
            if (sliderName == "sliderNum")
            {
                var currentNum = (int)value;
                if (_deletedItems.ContainsKey(_currentItem))
                    _deletedItems[_currentItem] = currentNum;
                    
                UpdateDeleteStateUI(currentNum);
            }
        }

        public void OnItemsRefreshed()
        {
            _inventoryController.ExitDeleteState();
        }

        private void ClearState()
        {
            // 移除添加减少按钮点击
            _inventoryController.OnButtonClickEvent -= OnButtonClickEvent;
            _inventoryController.OnInputFieldValueChangedEvent -= OnInputFieldValueChangedEvent;
            _inventoryController.OnSliderValueChangedEvent -= OnSliderValueChanged;
            
            // 清理删除缓存
            _deletedItems.Clear();
            _currentItem = null;
            
            // 重置输入框数量
            _inventoryDeleteViewModel.InputFieldDeleteNum.Value = DefaultDeleteMinNum.ToString();
            // 重置滑动条数量
            _inventoryDeleteViewModel.DeleteSliderNum.Value = DefaultDeleteMinNum;
            // 隐藏删除区域
            _inventoryDeleteViewModel.DeleteAreaActive.Value = false;
        }

        public Task Exit()
        {
            ClearState();
            return Task.CompletedTask;
        }
    }
}
