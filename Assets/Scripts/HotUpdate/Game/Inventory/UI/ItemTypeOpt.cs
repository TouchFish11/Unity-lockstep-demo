using System;
using System.Threading.Tasks;
using Core.UI;
using HotUpdate.Common.Config.Item;
using UnityEngine;
using UnityEngine.UI;
using Logger = Core.Log.Logger;

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
            if(icon)
                imgIcon.sprite = icon;
            togOpt.group = group;
        }

        public void Select()
        {
            togOpt.isOn = true;
        }

        protected override async void OnToggleValueChanged(string togName, bool isOn)
        {
            try
            {
                if (togName == nameof(togOpt) && isOn)
                {
                    await OnItemTypeOptChange?.Invoke(ItemType);
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"{nameof(ItemTypeOpt)}: {e.Message}");
            }
        }

        protected override void OnDisable()
        {
            OnItemTypeOptChange = null;
        }
    }
}
