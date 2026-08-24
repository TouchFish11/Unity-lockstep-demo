using UnityEngine.InputSystem;

namespace Core.Inputs
{
    /// <summary>
    /// 绑定信息
    /// </summary>
    public readonly struct BindingInfo
    {
        private InputBinding InputBinding { get; }
        
        public string ActionName => InputBinding.action;
        
        /// <summary>
        /// 绑定名称，若是单绑定则显示操作名称，否则显示绑定的名称
        /// </summary>
        public string BindingName => string.IsNullOrEmpty(InputBinding.name) ? ActionName : InputBinding.name;
        
        /// <summary>
        /// 绑定的设备键位
        /// </summary>
        public string DisplayKey => InputControlPath.ToHumanReadableString(InputBinding.effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice);

        public BindingInfo(InputBinding inputBinding)
        {
            InputBinding = inputBinding;
        }

        public override string ToString()
        {
            return $"{BindingName} ({InputBinding.effectivePath}) - Display Key: {DisplayKey}";
        }
    }
}
