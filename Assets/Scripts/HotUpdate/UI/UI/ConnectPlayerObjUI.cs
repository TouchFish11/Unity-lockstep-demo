using Core.UI;
using UnityEngine.UI;

namespace HotUpdate.UI.UI
{
    public class ConnectPlayerObjUI : UIBehaviourBase
    {
        [InjectUI] public Text txtPlayerIdInfo;
        
        public void Init(int clientId)
        {
            txtPlayerIdInfo.text = clientId.ToString();
        }
    }
}
