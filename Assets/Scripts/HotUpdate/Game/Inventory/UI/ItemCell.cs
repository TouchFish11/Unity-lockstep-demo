using System;
using System.Threading.Tasks;
using Core.UI;
using HotUpdate.Common.Config.Item;
using TMPro;
using UnityEngine.UI;

namespace HotUpdate.Game.Inventory.UI
{
    /// <summary>
    /// 物品格子
    /// </summary>
    public class ItemCell : UIBehaviourBase
    {
        [InjectUI] private Image imgBkQuality;
        [InjectUI] private Image imgIcon;
        [InjectUI] private TextMeshProUGUI txtNumOrLv;
        [InjectUI] private Button btnCell;
        private ItemDTO itemDTO;
        
        public event Func<ItemDTO, Task> OnSelect;
        
        /// <summary>
        /// 初始化物品
        /// </summary>
        /// <param name="itemDTO"></param>
        public void InitItem(ItemDTO itemDTO)
        {
            imgBkQuality.color = itemDTO.qualityBk;
            imgIcon.sprite = itemDTO.icon;
            // 非圣遗物返回数量，圣遗物返回强化等级
            txtNumOrLv.text = itemDTO.itemType != EItemType.HolyRelic ? itemDTO.itemNumOrLv.ToString() : $"+{itemDTO.itemNumOrLv}";
            this.itemDTO = itemDTO;
        }

        /// <summary>
        /// 选中当前物品格子
        /// </summary>
        public void Select()
        {
            OnSelect?.Invoke(itemDTO);
        }

        protected override void OnButtonClick(string btnName)
        {
            if (btnName == nameof(btnCell))
            {
                Select();
            }
        }

        protected override void OnDisable()
        {
            OnSelect = null;
        }
    }
}
