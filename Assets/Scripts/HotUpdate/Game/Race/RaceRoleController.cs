using Core.Inputs;
using Core.Log;
using Core.Math;
using Core.Net.Protocols.FSync.Messages;
using Core.Net.SyncModule.Interface;
using UnityEngine;
using UnityEngine.InputSystem;
using Logger = Core.Log.Logger;

namespace HotUpdate.Game.Race
{
    public class RaceRoleController : MonoBehaviour, INetObject
    {
        [SerializeField] private float speed = 1;
        
        private IInputSystem _inputSystem;
        private PlayerInput _playerInput;
        
        private byte opt;
        private int arg1;
        private int arg2;
        private int arg3;
        
        private FixedVector3 _logicPos;     // 当前逻辑帧位置
        private FixedVector3 prevLogicPos;   // 上一帧的逻辑位置
        private float lerpTime;         // 当前逻辑帧内已过去的时间比例 0~1
        private Vector3 _renderPos;
        
        private const float s_logicDeltaTime = 0.06f;
        
        public int ClientId { get; private set; }

        public void Init(int clientId, IInputSystem inputSystem, bool isApplyInput)
        {
            ClientId = clientId;
            if (isApplyInput)
            {
                _inputSystem = inputSystem;
                _playerInput = GetComponent<PlayerInput>();
                _inputSystem.InitPlayerInput(_playerInput, InputCallback);
                _inputSystem.Enable();
            }
        }

        private void InputCallback(InputAction.CallbackContext context)
        {
            var action = context.action;
            switch (action.name)
            {
                case ActionConfigs.Move:
                    if (context.phase == InputActionPhase.Performed)
                    {
                        opt = 1;
                        var value = action.ReadValue<Vector2>();
                        var dir = new Vector3(value.x, 0, value.y);
                        arg1 = (int)Fixed64.FromFloat(dir.x).RawValue;
                        arg2 = (int)Fixed64.FromFloat(dir.y).RawValue;
                        arg3 = (int)Fixed64.FromFloat(dir.z).RawValue;
                    }
                    else if(context.phase == InputActionPhase.Canceled)
                    {
                        opt = 1;
                        arg1 = (int)Fixed64.Zero.RawValue;
                        arg2 = (int)Fixed64.Zero.RawValue;
                        arg3 = (int)Fixed64.Zero.RawValue;
                    }
                    break;
            }
        }

        public void CollectInput(OptMessage optMessage)
        {
            optMessage.OptType = opt;
            optMessage.Arg1 = arg1;
            optMessage.Arg2 = arg2;
            optMessage.Arg3 = arg3;
        }

        public void SyncFrame(OptMessage optMessage)
        {
            // 转换速度和时间为定点数
            var speedFixed = Fixed64.FromFloat(speed);
            var deltaFixed = Fixed64.FromFloat(s_logicDeltaTime);
            var distance = speedFixed * deltaFixed;   // 内部已右移，得到定点距离
            // 构造方向向量（定点数）
            var direction = new FixedVector3(Fixed64.FromRaw(optMessage.Arg1), Fixed64.FromRaw(optMessage.Arg2), Fixed64.FromRaw(optMessage.Arg3));
            // 更新逻辑位置
            prevLogicPos = _logicPos;
            _logicPos += direction * distance;
            lerpTime = 0;
            
            Logger.LogDebug(ELogTags.Network, $"逻辑位置：{_logicPos}, 渲染位置：{_logicPos.ToVector3()}");
        }

        private void Update()
        {
            lerpTime += Time.deltaTime / s_logicDeltaTime;    // 累积到 1 表示这一逻辑帧走完
            transform.position = Vector3.Lerp(prevLogicPos.ToVector3(), _logicPos.ToVector3(), Mathf.Clamp01(lerpTime));
        }
    }
}
