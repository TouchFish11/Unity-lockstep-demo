using Core.AssetBundles.Management;
using Core.UI.MVC;

namespace HotUpdate.UI.Tip
{
    /// <summary>
    /// 提示界面数据
    /// </summary>
    public class TipModel : UIModel
    {
        public PoolObject ConfirmContent { get; set; }

        public override void ClearData()
        {
            ConfirmContent.Collect();
            ConfirmContent = default;
        }
    }
}
