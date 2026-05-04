using System;

namespace HotUpdate.UI.Tip.Update
{
    using Task = System.Threading.Tasks.Task;
    
    public class UpdateTipController : TipController
    {
        /// <summary>
        /// 确认事件
        /// </summary>
        public event Action OnSure;

        protected override Task OnActive()
        {
            return Task.CompletedTask;
        }

        protected override Task OnInactivate()
        {
            return Task.CompletedTask;
        }

        protected override Task OnInit()
        {
            return Task.CompletedTask;
        }

        public void SetUpdateMessage(string message)
        {
            //view.SetUpdateTip(message);
        }

        /// <summary>
        /// 设置提示信息
        /// </summary>
        /// <param name="isActive">false则隐藏，tip忽略；true则显示tip的内容</param>
        /// <param name="tip">显示的文本</param>
        public void SetTipActive(bool isActive, string tip = "")
        {
            //view.SetTipActive(isActive, tip);
        }

        protected override void OnButtonClick(string btnName)
        {
            if (btnName == nameof(view.btnOk))
            {
                uiManager.DestroyView(panelId);
                OnSure?.Invoke();
                OnSure = null;
            }
        }
    }
}
