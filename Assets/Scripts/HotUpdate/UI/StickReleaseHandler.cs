using Core.DI;
using Core.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HotUpdate.UI
{
    public class StickReleaseHandler : UIBehaviourBase
    {
        [Inject] private IUIManager uiManager;
        
        protected override void OnPointerUp(PointerEventData eventData)
        {

        }
    }
}
