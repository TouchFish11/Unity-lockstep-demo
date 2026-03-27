using System.Collections.Generic;
using System.Text;
using Core.UI.MVC;
using HotUpdate.Main.Chat;

namespace HotUpdate.Main.UI
{
    public class MainModel : UIModel
    {
        private List<FriendUI> friendUis = new List<FriendUI>();
        private readonly List<ChatUI> chatUis = new List<ChatUI>();
        private readonly StringBuilder _currentInput = new(16);

        public void InputStr(string str)
        {
            _currentInput.Append(str);
        }

        public void Cache(ChatUI chatUI)
        {
            chatUis.Add(chatUI);
        }
        
        public string GetChatInput() => _currentInput.ToString();
    }
}
