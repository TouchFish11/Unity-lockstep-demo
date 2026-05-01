using System.Collections.Generic;
using System.Threading.Tasks;
using HotUpdate.Common.Items;
using UnityEngine;

namespace HotUpdate.Game.Inventory.UI.State
{
    /// <summary>
    /// 背包界面删除状态
    /// </summary>
    public class InventoryDeleteState : AbstractInventoryState
    {
        private readonly Dictionary<long, int> _idToDeleteNumMap = new();
        
        public InventoryDeleteState(InventoryModel model, InventoryPanel view, InventoryController inventoryController) : 
            base(model, view, inventoryController)
        {
            
        }
        
        protected override Task OnEnter()
        {
            // 激活删除区域
            view.DeleteArea.gameObject.SetActive(true);
            // 监听格子的点击逻辑
            foreach (var itemCell in model.GridGenerator.GetAllCell())
            {
                itemCell.SwitchAction(ItemCell.EGridAction.Delete);
            }
            
            return Task.CompletedTask;
        }
        
        protected override Task OnItemClick(Item item)
        {
            var currentItemDelNum = _idToDeleteNumMap.GetValueOrDefault(item.persistentId, 1);
            // 更新数量框显示
            view.inputFieldDeleteNum.text = currentItemDelNum.ToString();
            
            
            
            
            
        }

        protected override Task OnExit()
        {
            // 隐藏删除区域
            view.DeleteArea.gameObject.SetActive(false);
            // 监听格子的点击逻辑
            foreach (var itemCell in model.GridGenerator.GetAllCell())
            {
                itemCell.SwitchAction(ItemCell.EGridAction.Normal);
            }
            return Task.CompletedTask;
        }
    }
}
