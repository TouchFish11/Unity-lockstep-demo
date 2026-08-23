using System.Collections.Generic;
using Core.UI;
using Core.UI.ViewController;
using TMPro;
using UnityEngine.UI;

namespace HotUpdate.UI
{
    public class MainView : UIView
    {
        [InjectUI] public Button btnInventory;
        [InjectUI] public Button btnAddItem;
        [InjectUI] public Button btnConnect;
        [InjectUI] public Button btnMatch;
        [InjectUI] public ScrollRect svOnline;
        [InjectUI] public TextMeshProUGUI txtSelfId;
        [InjectUI] public TextMeshProUGUI txtRtt;
        
        public Dictionary<int, ConnectPlayerObjUI> ConnectPlayers { get; } = new();
        
        /// <summary>
        /// 设置自己客户端ID
        /// </summary>
        /// <param name="clientId"></param>
        public void SetSelfClientId(int clientId)
        {
            txtSelfId.text = $"{clientId}";
        }

        public void SetTcpRtt(long clientRtt)
        {
            txtRtt.text = $"{clientRtt}ms";
        }
    }
}
