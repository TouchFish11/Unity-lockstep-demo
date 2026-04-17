using System;
using System.Threading.Tasks;
using Core.UI;
using HotUpdate.Common.Config.Item;
using UnityEngine;
using UnityEngine.UI;

namespace HotUpdate.Game.Inventory.UI
{
    /// <summary>
    /// 物品类型选项
    /// </summary>
    public class ItemTypeOpt : UIBehaviourBase
    {
        [InjectUI] private Image imgIcon;
        [InjectUI] private Toggle togOpt;
        
        public event Func<EItemType, Task> OnItemTypeOptChange;
        
        public EItemType ItemType { get; private set; }
        
        public void InitOption(EItemType itemType, Sprite icon, ToggleGroup group)
        {
            ItemType = itemType;
            imgIcon.sprite = icon;
            togOpt.group = group;
        }

        public void Select()
        {
            togOpt.isOn = true;
        }

        protected override void OnToggleValueChanged(string togName, bool isOn)
        {
            if (togName == nameof(togOpt) && isOn)
            {
                OnItemTypeOptChange?.Invoke(ItemType);
            }
        }
    }
}
