using Core.UI;
using HotUpdate.Game.Race.Logic;
using HotUpdate.Game.Race.View;
using TMPro;
using UnityEngine.UI;

namespace HotUpdate.UI
{
    public class SkillKeyUI : UIBehaviourBase
    {
        [InjectUI] private Button btnSkill;
        [InjectUI] private Image imgCooldown;
        [InjectUI] private TextMeshProUGUI txtCooldown;
        
        private ViewAvatar _viewAvatar;

        protected override void Awake()
        {
            base.Awake();
            imgCooldown.gameObject.SetActive(false);
            txtCooldown.gameObject.SetActive(false);
        }

        public void Init(ViewAvatar viewAvatar)
        {
            _viewAvatar = viewAvatar;
        }

        protected override void OnButtonClick(string btnName)
        {
            if (btnName == nameof(btnSkill) && _viewAvatar != null)
            {
                _viewAvatar.TryCastSkill();
            }
        }

        private void Update()
        {
            SetCooldown();
        }

        private void SetCooldown()
        {
            if(!_viewAvatar)
                return;

            // 剩余帧
            var remaining = _viewAvatar.GetCooldown(AbilityTable.AoeSkillId);
            if (remaining != 0)
            {
                imgCooldown.gameObject.SetActive(true);
                txtCooldown.gameObject.SetActive(true);
                
                imgCooldown.fillAmount = _viewAvatar.GetCooldown(AbilityTable.AoeSkillId) / (float)AbilityTable.Get(AbilityTable.AoeSkillId).Cooldown;
                txtCooldown.text = $"{remaining * FrameSyncConfig.LogicFrameSeconds:F1}";
            }
            else
            {
                imgCooldown.gameObject.SetActive(false);
                txtCooldown.gameObject.SetActive(false);
            }
        }

        protected override void OnDestroy()
        {
            _viewAvatar = null;
        }
    }
}
