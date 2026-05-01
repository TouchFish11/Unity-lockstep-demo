using System;
using Core.UI;
using HotUpdate.Base.Grid;
using HotUpdate.Base.Icon;
using HotUpdate.Base.Items;
using HotUpdate.Common.Items;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HotUpdate.Game.Inventory.UI
{
    /// <summary>
    /// 物品格子
    /// </summary>
    public class ItemCell : UIBehaviourBase, IGridBase<Item>
    {
        public enum EGridAction
        {
            Normal,
            Delete,
        }
        
        [InjectUI] private Image imgBkQuality;
        [InjectUI] private Image imgIcon;
        [InjectUI] private TextMeshProUGUI txtNumOrLv;
        [InjectUI] private Button btnCell;
        [InjectUI] private Image imgHighlight;
        [InjectUI] private Button btnCancelDelete;
        
        // 物品对象
        private Item _item;
        // 物品格子当前行为
        private EGridAction _gridAction = EGridAction.Normal;
        // 是否将被删除
        private bool isDelete;
        
        [InjectUI(1)] private RectTransform New { get; set; }
        
        public bool Selected { get; set; }
        
        /// <summary>
        /// 物品点击事件
        /// </summary>
        public event Action<Item> OnClick;

        protected override void Awake()
        {
            base.Awake();
            // 默认隐藏
            imgHighlight.gameObject.SetActive(false);
            btnCancelDelete.gameObject.SetActive(false);
            New.gameObject.SetActive(false);
        }

        /// <summary>
        /// 选中当前物品格子
        /// </summary>
        public void TriggerClick()
        {
            switch (_gridAction)
            {
                case EGridAction.Normal:
                {
                    if (_item.isNew)
                    {
                        // 隐藏New标志
                        New.gameObject.SetActive(false);
                        _item.isNew = false;
                    }
                    break;
                }
                case EGridAction.Delete:
                    Selected = !Selected;
                    btnCancelDelete.gameObject.SetActive(Selected);
                    break;
            }

            OnClick?.Invoke(_item);
        }

        /// <summary>
        /// 初始化物品格子
        /// </summary>
        /// <param name="item"></param>
        /// <param name="iconProvider"></param>
        public void InitGrid(Item item, IIconProvider iconProvider)
        {
            imgBkQuality.color = ItemFormatter.GetBkQualityColor(item);
            imgIcon.sprite = iconProvider.TryGetIcon(item.itemConfig.icon, out var icon) ? icon : null;
            // 根据物品的类型返回不同的数值格式化内容
            txtNumOrLv.text = ItemFormatter.GetItemNumOrLevel(item);
            // 是否是新物品
            New.gameObject.SetActive(item.isNew);
            _gridAction = EGridAction.Normal;
            _item = item;
        }

        /// <summary>
        /// 切换格子行为
        /// </summary>
        /// <param name="gridAction"></param>
        public void SwitchAction(EGridAction gridAction)
        {
            _gridAction = gridAction;
        }

        protected override void OnButtonClick(string btnName)
        {
            if (btnName == nameof(btnCell))
            {
                TriggerClick();
            }
            else if(btnName == nameof(btnCancelDelete))
            {
                
            }
        }

        protected override void OnDisable()
        {
            imgHighlight?.gameObject.SetActive(false);
            OnClick = null;
            _item = null;
        }
        
        protected override void OnPointerEnter(PointerEventData eventData)
        {
            imgHighlight?.gameObject.SetActive(true);
        }

        protected override void OnPointerExit(PointerEventData eventData)
        {
            imgHighlight?.gameObject.SetActive(false);
        }
    }
}
