using Core.UI;
using TMPro;

namespace HotUpdate.Main.Chat
{
    public class FriendUI : UIBehaviourBase
    {
        [Core.DI.Inject] private TextMeshProUGUI txtID;

        public void SetFriendUI(int clientID)
        {
            txtID.text = $"{clientID}";
        }
    }
}
