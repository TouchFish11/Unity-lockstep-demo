using Core.DI;
using Core.UI;
using HotUpdate.Game.Race;
using HotUpdate.Game.Race.View;
using TMPro;
using UnityEngine;

namespace HotUpdate.UI
{
    public class StatusHUD : UIBehaviourBase
    {
        [InjectUI] public TextMeshProUGUI txtWorldPos;
        [Inject] private IUIManager _uiManager;

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
            
            var pos = _viewAvatar.transform.position;
            txtWorldPos.text = $"Pos:({pos.x:F2},{pos.y:F2},{pos.z:F2})";
            FollowTarget(pos + _offset);
        }
    }
}
