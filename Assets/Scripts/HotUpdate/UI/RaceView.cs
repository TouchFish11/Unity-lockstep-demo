using System.Threading.Tasks;
using Core.UI;
using Core.UI.ViewController;
using HotUpdate.Game.Race.View;
using UnityEngine.UI;

namespace HotUpdate.UI
{
    public class RaceView : UIView
    {
        [InjectUI] public Text txtTcpNetRTTInfo;
        [InjectUI] public Text txtFrameRateInfo;
        [InjectUI] public Text txtFrameIDInfo;
        [InjectUI] public Button btnLeaveOrReConnect;
        [InjectUI] public Button btnSkill;

        private SkillKeyUI _skillKeyUI;

        protected override void Awake()
        {
            base.Awake();
            _skillKeyUI = this.GetComponentInChildren<SkillKeyUI>();
        }

        public void Init(ViewAvatar viewAvatar)
        {
            _skillKeyUI.Init(viewAvatar);
        }

        public override Task Destroy()
        {
            _skillKeyUI = null;
            return base.Destroy();
        }
    }
}
