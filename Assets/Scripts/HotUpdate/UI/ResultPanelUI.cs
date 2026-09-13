using System;
using Core.UI;
using TMPro;
using UnityEngine.UI;

namespace HotUpdate.UI
{
    public class ResultPanelUI : UIBehaviourBase
    {
        [InjectUI] private TextMeshProUGUI txtResult;   // "胜利" / "失败"
        [InjectUI] private Button btnReturn;            // 返回大厅按钮

        private Action _onReturn;

        protected override void OnButtonClick(string btnName)
        {
            if (btnName == nameof(btnReturn))
                _onReturn?.Invoke();
        }

        public void Init(bool win, Action onReturn)
        {
            txtResult.text = win ? "胜利" : "失败";
            _onReturn = onReturn;
        }
    }
}