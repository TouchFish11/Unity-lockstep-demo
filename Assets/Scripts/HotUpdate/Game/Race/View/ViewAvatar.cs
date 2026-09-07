using Core.Inputs;
using Core.Math;
using Core.Net.Protocols.FSync;
using Core.Net.SyncModule.Interface;
using Core.Time;
using HotUpdate.Game.Race.Logic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HotUpdate.Game.Race.View
{
    public class ViewAvatar : MonoBehaviour, INetObject
    {
        private LogicAvatar _logic;
        private float _lerpTime;
        private int _lastVersion = -1;
        private const float LogicFrameTime = 0.06f;     // 比逻辑 tick 的 0.066f 略小，让 _lerpTime 在 66ms 内能稳到 1（甚至略超被钳），保证位置追平逻辑位置。
        
        private FixedVector3 _pendingDir;   // 本地玩家当前移动方向

        public void Bind(LogicAvatar logic)
        {
            _logic = logic;
            transform.position = logic.Position.ToVector3();
        }
        
        public void InitInput(IInputSystem inputSystem, PlayerInput playerInput)
        {
            inputSystem.InitPlayerInput(playerInput, InputCallback);
            inputSystem.Enable();
        }
        
        private void InputCallback(InputAction.CallbackContext context)
        {
            if (context.action.name != ActionConfigs.Move) 
                return;

            if (context.phase == InputActionPhase.Performed)
            {
                var value = context.ReadValue<Vector2>();
                _pendingDir = new FixedVector3(Fixed64.FromFloat(value.x), Fixed64.Zero, Fixed64.FromFloat(value.y));
            }
            else if (context.phase == InputActionPhase.Canceled)
            {
                _pendingDir = FixedVector3.Zero;
            }
        }

        private void Update()
        {
            if (_logic == null) 
                return;

            if (_logic.Version != _lastVersion) // 逻辑位置更新了，重置插值
            {
                _lastVersion = _logic.Version;
                _lerpTime = 0;
            }
            
            _lerpTime += TimeUtil.DeltaTime / LogicFrameTime;
            transform.position = Vector3.Lerp(_logic.PrevPosition.ToVector3(), _logic.Position.ToVector3(), Mathf.Clamp01(_lerpTime));
        }

        public void CollectInput(ref InputCommand cmd)
        {
            cmd.optType = EOptType.Move;
            cmd.dir = _pendingDir;
        }
    }
}