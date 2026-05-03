using System.Collections.Generic;
using System.Threading.Tasks;
using Core.DI;
using HotUpdate.Base.Inventory;
using HotUpdate.Common.Items;
using Logger = Core.Log.Logger;

namespace HotUpdate.Game.Inventory.UI.State
{
    /// <summary>
    /// 背包界面删除状态
    /// </summary>
    public class InventoryDeleteState : IInventoryState
    {
        [Inject] private IInventoryManager _inventoryManager;
        
        private readonly InventoryModel _inventoryModel;
        private readonly InventoryController _inventoryController;
        private readonly InventoryPanel _inventoryPanel;
        
        private readonly Dictionary<Item, int> _deletedItems = new();
        
        // 当前正在操作的物品对象
        private Item _currentItem;
        /// 默认最小删除数量
        private const int DefaultDeleteMinNum = 1;
        
        public InventoryDeleteState(InventoryModel model, InventoryPanel view, InventoryController inventoryController)
        {
            _inventoryModel = model;
            _inventoryPanel = view;
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
            _inventoryPanel.DeleteArea.gameObject.SetActive(true);
            return Task.CompletedTask;
        }

        public Task OnItemClick(Item item)
        {
            // 先记录当前操作的物品
            _currentItem = item;
            // 更新滑动条的最最大/最小数量
            var itemData = _inventoryManager.GetData(_currentItem);
            _inventoryPanel.sliderNum.maxValue = itemData.itemNum;
            _inventoryPanel.sliderNum.minValue = DefaultDeleteMinNum;

            // 点击相同格子移除销毁状态
            if (_deletedItems.Remove(item))
            {
                // 隐藏删除盒的输入
                _inventoryPanel.SetDeleteBoxActive(false);
            }
            // 新增销毁物品
            else
            {
                _deletedItems.Add(item, DefaultDeleteMinNum);
                if (item.itemConfig.isPile)
                {
                    // 激活删除盒的输入
                    _inventoryPanel.SetDeleteBoxActive(true);
                    UpdateDeleteStateUI(DefaultDeleteMinNum);
                }
                else
                {
                    // 隐藏删除盒的输入
                    _inventoryPanel.SetDeleteBoxActive(false);
                }
            }
            return Task.CompletedTask;
        }

        private async void OnButtonClickEvent(string btnName)
        {
            if (btnName == nameof(_inventoryPanel.btnAdd))
            {
                // 能进入这里的逻辑，一定是可堆叠的物品
                var currentNum  = _deletedItems[_currentItem];
                ++currentNum;
                // 同步字典缓存
                _deletedItems[_currentItem] = currentNum;
                UpdateDeleteStateUI(currentNum);
            }
            else if (btnName == nameof(_inventoryPanel.btnSub))
            {
                // 能进入这里的逻辑，一定是可堆叠的物品
                var currentNum = _deletedItems[_currentItem];
                --currentNum;
                // 同步字典缓存
                _deletedItems[_currentItem] = currentNum;
                UpdateDeleteStateUI(currentNum);
            }
            else if (btnName == nameof(_inventoryPanel.btnDelete))
            {
                // 删除物品
                _inventoryController.ExecuteDelete(_deletedItems);
                // 刷新当前界面
                _inventoryController.UpdateItemsByType(_inventoryModel.CurrentItemType);
                // 删除完毕后，退出删除状态
                await _inventoryController.ExitDeleteState();
                
                // 打开删除确认界面(可选)
                // ...
            }
            else if (btnName == nameof(_inventoryPanel.btnCancelDelete))
            {
                // 删除完毕后，退出删除状态
                await _inventoryController.ExitDeleteState();
            }
            else if (btnName == nameof(_inventoryPanel.btnMin))
            {
                _deletedItems[_currentItem] = DefaultDeleteMinNum;
                UpdateDeleteStateUI(DefaultDeleteMinNum);
            }
            else if (btnName == nameof(_inventoryPanel.btnMax))
            {
                var itemData = _inventoryManager.GetData(_currentItem);
                _deletedItems[_currentItem] = itemData.itemNum;
                UpdateDeleteStateUI(itemData.itemNum);
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
                _inventoryPanel.btnAdd.enabled = false;
                _inventoryPanel.btnMax.enabled = false;
                _inventoryPanel.btnSub.enabled = false;
                _inventoryPanel.btnMin.enabled = false;
                    
                _inventoryPanel.inputFieldDeleteNum.text = DefaultDeleteMinNum.ToString();
                _inventoryPanel.sliderNum.value = DefaultDeleteMinNum;
                return;
            }
                
            // 删除数量不允许超过拥有数量
            if (currentNum == itemData.itemNum)
            {
                // 禁用增加按钮
                _inventoryPanel.btnAdd.enabled = false;
                // 禁用最大按钮
                _inventoryPanel.btnMax.enabled = false;

                _inventoryPanel.btnSub.enabled = true;
                _inventoryPanel.btnMin.enabled = true;
            }
            // 删除数量不允许为负数或0
            else if(currentNum == 1)
            {
                // 禁用最小按钮
                _inventoryPanel.btnSub.enabled = false;
                // 禁用减少按钮
                _inventoryPanel.btnMin.enabled = false;
                    
                _inventoryPanel.btnAdd.enabled = true;
                _inventoryPanel.btnMax.enabled = true;
            }
            else
            {
                _inventoryPanel.btnAdd.enabled = true;
                _inventoryPanel.btnMax.enabled = true;
                _inventoryPanel.btnSub.enabled = true;
                _inventoryPanel.btnMin.enabled = true;
            }
                
            // 更新输入框显示
            if(_inventoryPanel.inputFieldDeleteNum.text != currentNum.ToString())
                _inventoryPanel.inputFieldDeleteNum.text = currentNum.ToString();
            // 更新滑动条显示
            if((int)_inventoryPanel.sliderNum.value != currentNum)
                _inventoryPanel.sliderNum.value = currentNum;
        }
        
        private void OnInputFieldValueChangedEvent(string inputFieldName, string inputFieldValue)
        {
            if (inputFieldName == nameof(_inventoryPanel.inputFieldDeleteNum))
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
            if (sliderName == nameof(_inventoryPanel.sliderNum))
            {
                if (_inventoryPanel.sliderNum.wholeNumbers)
                {
                    var currentNum = (int)value;
                    if (_deletedItems.ContainsKey(_currentItem))
                        _deletedItems[_currentItem] = currentNum;
                    
                    UpdateDeleteStateUI(currentNum);
                }
            }
        }

        public void OnItemsRefreshed()
        {
            _deletedItems.Clear();
            _currentItem = null;
            // 重置输入框数量
            _inventoryPanel.inputFieldDeleteNum.text = DefaultDeleteMinNum.ToString();
            // 重置滑动条数量
            _inventoryPanel.sliderNum.value = DefaultDeleteMinNum;
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
            _inventoryPanel.inputFieldDeleteNum.text = DefaultDeleteMinNum.ToString();
            // 重置滑动条数量
            _inventoryPanel.sliderNum.value = DefaultDeleteMinNum;
            // 隐藏删除区域
            _inventoryPanel.DeleteArea.gameObject.SetActive(false);
        }

        public Task Exit()
        {
            ClearState();
            return Task.CompletedTask;
        }
    }
}
