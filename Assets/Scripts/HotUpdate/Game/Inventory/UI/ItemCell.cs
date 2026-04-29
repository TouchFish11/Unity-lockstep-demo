using System;
using Core.Log;
using Core.UI;
using Core.Utility;
using HotUpdate.Base.Grid;
using HotUpdate.Base.Icon;
using HotUpdate.Base.Items;
using HotUpdate.Common.Config.Item;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HotUpdate.Game.Inventory.UI
{
    /// <summary>
    /// 物品格子
    /// </summary>
    public class ItemCell : UIBehaviourBase, IGridBase<Item>
    {
        [InjectUI] private Image imgBkQuality;
        [InjectUI] private Image imgIcon;
        [InjectUI] private TextMeshProUGUI txtNumOrLv;
        [InjectUI] private Button btnCell;
        [InjectUI] private Image imgHighlight;
        
        // 物品对象
        private Item _item;
        
        /// <summary>
        /// 物品点击事件
        /// </summary>
        public event Action<Item> OnClick;

        protected override void Awake()
        {
            base.Awake();
            // 默认隐藏
            imgHighlight.gameObject.SetActive(false);
            // 为按钮添加鼠标进入/离开事件
            UIUtility.AddCustomEventListener(btnCell, EventTriggerType.PointerEnter, OnPointerEnter);
            UIUtility.AddCustomEventListener(btnCell, EventTriggerType.PointerExit, OnPointerExit);
        }

        /// <summary>
        /// 选中当前物品格子
        /// </summary>
        public void TriggerClick()
        {
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
            _item = item;
        }

        protected override void OnButtonClick(string btnName)
        {
            if (btnName == nameof(btnCell))
            {
                TriggerClick();
            }
        }

        private void OnPointerEnter(BaseEventData eventData)
        {
            imgHighlight?.gameObject.SetActive(true);
        }

        private void OnPointerExit(BaseEventData eventData)
        {
            imgHighlight?.gameObject.SetActive(false);
        }

        protected override void OnDisable()
        {
            imgHighlight?.gameObject.SetActive(false);
            OnClick = null;
            _item = null;
        }
    }
}
