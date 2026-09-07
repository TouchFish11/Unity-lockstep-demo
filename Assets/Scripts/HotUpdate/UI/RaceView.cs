using Core.UI;
using Core.UI.ViewController;
using UnityEngine.UI;

namespace HotUpdate.UI
{
    public class RaceView : UIView
    {
        [InjectUI] public Text txtTcpNetRTTInfo;
        [InjectUI] public Text txtFrameRateInfo;
        [InjectUI] public Text txtFrameIDInfo;
        [InjectUI] public Text txtRenderPosInfo;
        [InjectUI] public Text txtLogicPosInfo;
        [InjectUI] public Button btnLeaveOrReConnect;
    }
}
