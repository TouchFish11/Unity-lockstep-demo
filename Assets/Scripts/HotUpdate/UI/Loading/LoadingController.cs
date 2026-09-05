using System.Threading.Tasks;
using Core.UI.ViewController;

namespace HotUpdate.UI.Loading
{
    public class LoadingController : UIController<LoadingView>
    {
        protected override Task OnInit()
        {
            return Task.CompletedTask;
        }

        protected override Task OnActive()
        {
            return Task.CompletedTask;
        }

        protected override Task OnInactivate()
        {
            return Task.CompletedTask;
        }
    }
}
