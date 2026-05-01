using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.DI;
using Core.UI.MVC;
using HotUpdate.Base.Grid;
using HotUpdate.Common.Items;
using HotUpdate.Game.Inventory.UI.State;
using UnityEngine;

namespace HotUpdate.Game.Inventory.UI
{
    /// <summary>
    /// 背包界面控制器
    /// </summary>
    public class InventoryController : UIController<InventoryPanel, InventoryModel>
    {
        private readonly Dictionary<Type, IInventoryState> _inventoryStates = new();
        
        // 当前背包界面所处的状态
        private IInventoryState  _currentInventoryState;
        
        protected override async Task OnInit()
        {
            // 初始化背包界面所有状态
            _inventoryStates.Add(typeof(InventoryNormalState), DIContainer.Create<InventoryNormalState>(parameterValues: new object[]
            {
                model,
                view,
                this,
            }));
            _inventoryStates.Add(typeof(InventoryDeleteState), DIContainer.Create<InventoryDeleteState>(parameterValues: new object[]
            {
                model,
                view,
                this
            }));
            
            // 初始化工厂
            model.InitDetailPanelFactory();
            // 初始化格子生成器
            if (_inventoryStates[typeof(InventoryNormalState)] is InventoryNormalState normalState)
            {
                normalState.InitGridGenerator();
                // 创建选项
                await normalState.InitTypeOpt();
            }
        }
        
        protected override async Task OnActive()
        {
            await TransitionTo(typeof(InventoryNormalState));
        }

        protected override async Task OnInactivate()
        {
            await TransitionTo(null);
            // 先回收，再销毁，否则对象池会无法清理
            model.ClearOpt();
        }

        /// <summary>
        /// 过渡到指定状态
        /// </summary>
        /// <param name="nextState"></param>
        public async Task TransitionTo(Type nextState)
        {
            if(_currentInventoryState !=  null)
                await _currentInventoryState.Exit();

            if (nextState == null)
                _currentInventoryState = null;
            
            _currentInventoryState = _inventoryStates[nextState];
            if(_currentInventoryState != null)
                await _currentInventoryState.Enter();
        }
        
        public event Action<string> OnButtonClickEvent;
        
        protected override void OnButtonClick(string btnName)
        {
            OnButtonClickEvent?.Invoke(btnName);
        }

        public event Action<string, Vector2> OnScrollRectValueChangedEvent;
        
        protected override void OnScrollRectValueChanged(string scrollViewName, Vector2 pos)
        {
            OnScrollRectValueChangedEvent?.Invoke(scrollViewName, pos);
        }

        public event Action<string, int> OnDropdownValueChangedEvent;
        
        protected override void OnDropdownValueChanged(string dropdownName, int index)
        {
            OnDropdownValueChangedEvent?.Invoke(dropdownName, index);
        }

        protected override Task OnDestroy()
        {
            _inventoryStates.Clear();
            _currentInventoryState = null;
            return Task.CompletedTask;
        }
    }
}
