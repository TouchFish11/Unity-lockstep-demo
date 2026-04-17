using Core.UI.MVC;

namespace HotUpdate.UI.Tip
{
    public abstract class TipController<TView, TModel> : UIController<TView, TModel> where TView : IuiView where TModel : IuiModel
    {

    }
}
