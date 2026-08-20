using System;
using System.Threading.Tasks;
using Core.DI;
using Core.Log;
using Core.UI;
using Core.UI.ViewController;

namespace HotUpdate.UI.UI
{
    public class MainController : UIController<MainPanel>
    {
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

        protected override async void OnButtonClick(string btnName)
        {
            
        }
    }
}
