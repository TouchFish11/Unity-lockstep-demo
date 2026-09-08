using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Test22 : MonoBehaviour
{
    private PlayerControls controls;
    private PlayerInput playerInput;
    
    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        controls = new PlayerControls();
        playerInput.actions = controls.asset;
        playerInput.SwitchCurrentControlScheme(controls.AndroidScheme.name);
        controls.asset.Enable();
    }
    
    
    void OnEnable()
    {
        controls.Main.Enable();
        controls.Main.Move.performed += OnMove;
        controls.Main.Attack.performed += OnAttack;
    }
    
    private void OnMove(InputAction.CallbackContext context)
    {
        Vector2 move = context.ReadValue<Vector2>();
        Debug.Log(move);
        // 处理移动
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        foreach (var actionControl in context.action.controls)
        {
            Debug.Log(actionControl.path);
        }

        // 处理攻击
        Debug.Log($"攻击, Action Name:{context.action.name}");
    }
    
    void OnDisable()
    {
        controls.Main.Move.performed -= OnMove;
        controls.Main.Attack.performed -= OnAttack;
        controls.Main.Disable();
    }
}
