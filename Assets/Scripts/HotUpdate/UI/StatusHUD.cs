using Core.DI;
using Core.UI;
using HotUpdate.Game.Race;
using TMPro;
using UnityEngine;

namespace HotUpdate.UI
{
    public class StatusHUD : UIBehaviourBase
    {
        [InjectUI] public TextMeshProUGUI txtWorldPos;
        [Inject] private IUIManager _uiManager;

        [SerializeField] private Vector3 _offset;
        
        private RaceRoleController _raceRoleController;
        private Transform _parent;

        public void Init(RaceRoleController raceRoleController, Transform parent)
        {
            _raceRoleController = raceRoleController;
            _parent = parent;
        }

        private void FollowTarget(Vector3 pos)
        {
            UIUtility.WorldToLocalPointInRectangle(Camera.main, _uiManager.UICamera, _parent, this.gameObject, pos);
        }
        
        private void Update()
        {
            if(!_raceRoleController)
                return;
            
            var pos = _raceRoleController.transform.position;
            txtWorldPos.text = $"Pos:({pos.x:F2},{pos.y:F2},{pos.z:F2})";
            FollowTarget(pos + _offset);
        }
    }
}
