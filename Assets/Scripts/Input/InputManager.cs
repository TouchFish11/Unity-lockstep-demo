using System;
using UnityEngine;

namespace Input
{
    public class InputManager : MonoBehaviour, IInputEvent
    {
        public static InputManager Instance { get; private set; }
        private IPlayerInput _activeInput;

        public event Action<Vector2> OnMove
        {
            add => _activeInput.OnMove += value;
            remove => _activeInput.OnMove -= value;
        }
        
        public event Action OnJump
        {
            add => _activeInput.OnJump += value;
            remove => _activeInput.OnJump -= value;
        }
        
        public event Action OnDash
        {
            add => _activeInput.OnDash += value;
            remove => _activeInput.OnDash -= value;
        }
        
        public event Action OnInteract
        {
            add => _activeInput.OnInteract += value;
            remove => _activeInput.OnInteract -= value;
        }
        
        public event Action OnAttack
        {
            add => _activeInput.OnAttack += value;
            remove => _activeInput.OnAttack -= value;
        }
        
        private void Awake()
        {
            Instance = this;
            SwitchInput(new KeyboardInput());
        }

        public void SwitchInput(IPlayerInput activeInput)
        {
            _activeInput = activeInput;
        }

        private void Update()
        {
            _activeInput?.OnUpdateInput();
        }
    }
}
