using System;
using UnityEngine;

namespace Input
{
    using Input = UnityEngine.Input;

    public class KeyboardInput : IPlayerInput
    {
        public event Action<Vector2> OnMove;
        public event Action OnJump;
        public event Action OnDash;
        public event Action OnInteract;
        public event Action OnAttack;
    
        public void OnUpdateInput()
        {
            OnMove?.Invoke(new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")));
            
            if (Input.GetKeyDown(KeyCode.Space))
            {
                OnJump?.Invoke();
            }

            if (Input.GetMouseButtonDown(0))
            {
                OnAttack?.Invoke();
            }

            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                OnDash?.Invoke();
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                OnInteract?.Invoke();
            }
        }
    }
}
