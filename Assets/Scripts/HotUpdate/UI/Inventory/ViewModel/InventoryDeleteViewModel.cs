using System;
using Core.UI;

namespace HotUpdate.UI.Inventory.ViewModel
{
    /// <summary>
    /// 背包删除物品VM
    /// </summary>
    public class InventoryDeleteViewModel : IDisposable
    {
        public ReactiveProperty<bool> DeleteAreaActive { get; } = new();
        
        public ReactiveProperty<bool> DeleteBoxActive { get; } = new();

        public ReactiveProperty<(float min, float max)> DeleteSliderExtremum { get; } = new();
        
        public ReactiveProperty<float> DeleteSliderNum { get; } = new();

        public ReactiveProperty<bool> AddDeleteBtnEnalbe { get; } = new();
        
        public ReactiveProperty<bool> SubDeleteBtnEnalbe { get; } = new();

        public ReactiveProperty<bool> MinDeleteBtnEnalbe { get; } = new();
        
        public ReactiveProperty<bool> MaxDeleteBtnEnalbe { get; } = new();

        public ReactiveProperty<string> InputFieldDeleteNum { get; } = new();
        
        public void Dispose()
        {
            DeleteAreaActive.Dispose();
            DeleteBoxActive.Dispose();
            DeleteSliderExtremum.Dispose();
            DeleteSliderNum.Dispose();
            AddDeleteBtnEnalbe.Dispose();
            SubDeleteBtnEnalbe.Dispose();
            MinDeleteBtnEnalbe.Dispose();
            MaxDeleteBtnEnalbe.Dispose();
            InputFieldDeleteNum.Dispose();
        }
    }
}
