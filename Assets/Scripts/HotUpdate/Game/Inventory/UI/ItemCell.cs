using System;
using Core.UI;
using HotUpdate.Base.Grid;
using HotUpdate.Common.Config.Item;
using TMPro;
using UnityEngine.UI;

namespace HotUpdate.Game.Inventory.UI
{
    /// <summary>
    /// 物品格子
    /// </summary>
    public class ItemCell : UIBehaviourBase, IGridBase<ItemDTO>
    {
        [InjectUI] private Image imgBkQuality;
        [InjectUI] private Image imgIcon;
        [InjectUI] private TextMeshProUGUI txtNumOrLv;
        [InjectUI] private Button btnCell;
        private ItemDTO itemDTO;
        
        public event Action<ItemDTO> OnClick;
        
        /// <summary>
        /// 选中当前物品格子
        /// </summary>
        public void TriggerClick()
        {
            OnClick?.Invoke(itemDTO);
        }

        public void InitGrid(ItemDTO data)
        {
            imgBkQuality.color = data.qualityBk;
            if(data.icon)
                imgIcon.sprite = data.icon;
            // 非圣遗物返回数量，圣遗物返回强化等级
            txtNumOrLv.text = data.itemType != EItemType.HolyRelic ? data.itemNumOrLv.ToString() : $"+{data.itemNumOrLv}";
            itemDTO = data;
        }

        protected override void OnButtonClick(string btnName)
        {
            if (btnName == nameof(btnCell))
            {
                TriggerClick();
            }
        }

        protected override void OnDisable()
        {
            OnClick = null;
            itemDTO = null;
        }
    }
}
