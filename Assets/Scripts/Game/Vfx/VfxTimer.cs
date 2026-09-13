using System;
using Core.Time;
using UnityEngine;

namespace Game.Vfx
{
    public class VfxTimer : MonoBehaviour
    {
        [SerializeField] private float duration;

        private float _currentTime;
        public event Action overCallback;

        private void OnEnable()
        {
            _currentTime = 0;
        }

        private void Update()
        {
            _currentTime += TimeUtil.DeltaTime;
            if (_currentTime >= duration)
            {
                overCallback?.Invoke();
                overCallback = null;
            }
        }
    }
}
