using System.Collections.Generic;
using Core.UI;
using Core.UI.ViewController;
using TMPro;
using UnityEngine.UI;

namespace HotUpdate.UI
{
    public class MainView : UIView
    {
        [InjectUI] public Button btnReplay;
        [InjectUI] public Button btnInventory;
        [InjectUI] public Button btnAddItem;
        [InjectUI] public Button btnConnect;
        [InjectUI] public Button btnMatch;
        [InjectUI] public ScrollRect svOnline;
        [InjectUI] public TextMeshProUGUI txtSelfId;
        [InjectUI] public TextMeshProUGUI txtRtt;
        
        public Dictionary<int, ConnectPlayerObjUI> ConnectPlayers { get; } = new();
        
        /// <summary>
        /// 确认UI
        /// </summary>
        public ConfirmPanelUI ConfirmPanelUI { get; set; }
        
        /// <summary>
        /// 设置自己客户端ID
        /// </summary>
        /// <param name="clientId"></param>
        public void SetSelfClientId(int clientId)
        {
            txtSelfId.text = $"{clientId}";
        }

        /// <summary>
        /// 设置延迟
        /// </summary>
        /// <param name="clientRtt">-1为未连接，显示∞</param>
        public void SetTcpRtt(long clientRtt)
        {
            txtRtt.text = clientRtt != -1 ? $"{clientRtt}ms" : "∞";
        }
    }
}
