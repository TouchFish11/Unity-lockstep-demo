using System.Collections.Generic;
using System.Threading.Tasks;
using HotUpdate.Common.Items;
using Logger = Core.Log.Logger;

namespace HotUpdate.Game.Inventory.UI.State
{
    /// <summary>
    /// 背包界面删除状态
    /// </summary>
    public class InventoryDeleteState : AbstractInventoryState
    {
        private readonly Dictionary<Item, int> _deletedItems = new();
        
        // 当前正在操作的物品对象
        private Item _currentItem;
        /// 输入框默认显示数量
        private const int DefaultInputFieldValue = 1;
        
        public InventoryDeleteState(InventoryModel model, InventoryPanel view, InventoryController inventoryController) : 
            base(model, view, inventoryController)
        {
            
        }
        
        protected override Task OnEnter()
        {
            // 监听添加减少按钮点击
            inventoryController.OnButtonClickEvent += OnButtonClickEvent;
            inventoryController.OnInputFieldValueChangedEvent += OnInputFieldValueChangedEvent;
            // 激活删除区域
            view.DeleteArea.gameObject.SetActive(true);
            // 切换格子的点击逻辑，此时格子存在
            foreach (var itemCell in model.GridGenerator.GetAllCell())
            {
                itemCell.SetClick(ItemClick);
                itemCell.SwitchAction(ItemCell.EGridAction.Delete);
            }
            return Task.CompletedTask;
        }
        
        protected override Task OnItemClick(Item item)
        {
            // 先记录当前操作的物品
            _currentItem = item;
            
            if (!_deletedItems.Remove(item))
                _deletedItems.Add(item, 1);

            if (item.itemConfig.isPile)
            {
                // 激活删除盒的输入
                view.SetDeleteBoxActive(true);
                // 更新数量框显示
                view.inputFieldDeleteNum.text = DefaultInputFieldValue.ToString();
            }
            else
            {
                // 隐藏删除盒的输入
                view.SetDeleteBoxActive(false);
            }
            
            return Task.CompletedTask;
        }
        
        private async void OnButtonClickEvent(string btnName)
        {
            if (btnName == nameof(view.btnAdd))
            {
                // 能进入这里的逻辑，一定是可堆叠的物品
                var currentNum  = _deletedItems[_currentItem];
                var itemData = _inventoryManager.GetData(_currentItem);
                // 删除数量不允许超过拥有数量
                ++currentNum;
                if (currentNum == itemData.itemNum)
                {
                    // 禁用增加按钮
                    view.btnAdd.enabled = false;
                }
                else
                {
                    view.btnAdd.enabled = true;
                    view.btnSub.enabled = true;
                }
                
                // 同步字典缓存
                _deletedItems[_currentItem] = currentNum;
                // 同步更新输入框显示
                view.inputFieldDeleteNum.text = currentNum.ToString();
            }
            else if (btnName == nameof(view.btnSub))
            {
                // 能进入这里的逻辑，一定是可堆叠的物品
                var currentNum = _deletedItems[_currentItem];
                --currentNum;
                // 删除数量不允许为负数或0
                if (currentNum == 1)
                {
                    // 禁用减少按钮
                    view.btnSub.enabled = false;
                }
                else
                {
                    view.btnAdd.enabled = true;
                    view.btnSub.enabled = true;
                }
                
                // 同步字典缓存
                _deletedItems[_currentItem] = currentNum;
                // 同步更新输入框显示
                view.inputFieldDeleteNum.text = currentNum.ToString();
            }
            else if (btnName == nameof(view.btnDelete))
            {
                // 删除物品
                foreach (var (item, delNum) in _deletedItems)
                {
                    if (item.itemConfig.isPile)
                    {
                        _inventoryManager.DeleteItem(item.itemConfig.itemId, delNum);
                    }
                    else
                    {
                        _inventoryManager.DeleteItem(item.persistentId);
                    }
                }
                
                // 删除完毕后，退出删除状态
                await inventoryController.TransitionTo(typeof(InventoryNormalState));
                
                // 刷新当前界面
                await UpdateItemsByType(model.CurrentItemType);

                // 打开删除确认界面(可选)
                // ...
            }
            else if (btnName == nameof(view.btnCancelDelete))
            {
                // 回到正常状态
                await inventoryController.TransitionTo(typeof(InventoryNormalState));
            }
        }

        private void OnInputFieldValueChangedEvent(string inputFieldName, string inputFieldValue)
        {
            if (inputFieldName == nameof(view.inputFieldDeleteNum))
            {
                if(_currentItem == null)
                    return;
                
                // 能进入这里的逻辑，一定是可堆叠的物品
                // 转换成功才处理
                if (int.TryParse(inputFieldValue, out var currentNum))
                {
                    _deletedItems[_currentItem] = currentNum;
                }
                else
                {
                    Logger.Log($"Invalid input: {inputFieldValue}");
                }
            }
        }

        protected override Task OnExit()
        {
            // 移除添加减少按钮点击
            inventoryController.OnButtonClickEvent -= OnButtonClickEvent;
            inventoryController.OnInputFieldValueChangedEvent -= OnInputFieldValueChangedEvent;
            
            // 清理删除缓存
            _deletedItems.Clear();
            _currentItem = null;
            
            // 重置输入框数量
            view.inputFieldDeleteNum.text = DefaultInputFieldValue.ToString();
            // 隐藏删除区域
            view.DeleteArea.gameObject.SetActive(false);
            return Task.CompletedTask;
        }
    }
}
