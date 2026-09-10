using Core.DI;
using Core.UI;
using HotUpdate.Game.Race.Logic;
using HotUpdate.Game.Race.View;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HotUpdate.UI
{        
    public class StatusHUD : UIBehaviourBase
    {
        // 位置
        [InjectUI] public TextMeshProUGUI txtWorldPos;
        
        // 血量相关
        [InjectUI] public Image imgTop;
        [InjectUI] public Image imgFade;
        [InjectUI] public TextMeshProUGUI txtBlood;

        [Inject] private IUIManager _uiManager;
        
        // 自身偏移
        [SerializeField] private Vector3 _offset;
        
        private ViewAvatar _viewAvatar;
        private Transform _parent;

        public void Init(ViewAvatar viewAvatar, Transform parent)
        {
            _viewAvatar = viewAvatar;
            _parent = parent;
        }

        private void FollowTarget(Vector3 pos)
        {
            UIUtility.WorldToLocalPointInRectangle(Camera.main, _uiManager.UICamera, _parent, this.gameObject, pos);
        }
        
        private void Update()
        {
            if(!_viewAvatar)
                return;
            
            // 位置
            var pos = _viewAvatar.transform.position;
            txtWorldPos.text = $"Pos:({pos.x:F2},{pos.y:F2},{pos.z:F2})";
            
            // 血量
            imgTop.fillAmount = _viewAvatar.CurrentHp / LogicAvatar.MaxHp;
            imgFade.fillAmount = imgTop.fillAmount;
            txtBlood.text = $"{(int)_viewAvatar.CurrentHp}/{LogicAvatar.MaxHp}";
            
            // 跟随
            FollowTarget(pos + _offset);
        }
    }
}
