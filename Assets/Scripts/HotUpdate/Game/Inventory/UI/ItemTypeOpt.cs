using System;
using System.Threading.Tasks;
using Core.UI;
using HotUpdate.Common.Items;
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
        
        /// <summary>
        /// 选项类型切换事件
        /// </summary>
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
                    if (OnItemTypeOptChange != null)
                        await OnItemTypeOptChange?.Invoke(ItemType);
                }
            }
            catch (OperationCanceledException canceledException)
            {
                Logger.Log($"[{nameof(ItemTypeOpt)}]: Item type switch operator cancel, {canceledException.Message}");
            }
            catch (Exception e)
            {
                Logger.LogError($"[{nameof(ItemTypeOpt)}]: Item type switch fail, {e.Message}");
            }
        }

        protected override void OnDisable()
        {
            OnItemTypeOptChange = null;
        }
    }
}
