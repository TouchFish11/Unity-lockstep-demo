// #if UNITY_EDITOR
//
// using System;
// using System.IO;
// using Core.DI;
// using Core.Inputs;
// using Core.Inputs.Providers;
// using Core.Log;
// using Core.Registration;
// using Core.Utility;
// using TMPro;
// using Unity.VisualScripting;
// using UnityEngine;
// using UnityEngine.InputSystem;
// using UnityEngine.UI;
// using InputSystem = Core.Inputs.InputSystem;
// using Logger = Core.Log.Logger;
//
// public class Test : MonoBehaviour
// {
//     public Button btnNormalAttack;
//     public TextMeshProUGUI txtNormalAttack;
//     public Button btnMove_Up;
//     public TextMeshProUGUI txtMove_Up;
//     
//     private IInputSystem inputSystem;
//     public InputActionAsset asset;
//     private PlayerInput playerInput;
//
//     private static string s_inputPath;
//     
//     
//     private async void Awake()
//     {
//         await RegisterCore.InitCore();
//         s_inputPath = PathUtility.GetUserDataLocalSavePath(FileSources.InputActionLocalFileName);
//     }
//
//     private void OnEnable()
//     {
//         playerInput ??= this.AddComponent<PlayerInput>();
//         btnNormalAttack.onClick.AddListener(() => EditKey(ActionConfigs.NormalAttack));
//         btnMove_Up.onClick.AddListener(() => EditKey(ActionConfigs.Move, BindingSources.Up));
//     }
//
//     private async void Start()
//     {
//         inputSystem = DIContainer.Create<InputSystem>();
//         var overrideJson = string.Empty;
//         if (File.Exists(s_inputPath))
//         {
//             overrideJson = await File.ReadAllTextAsync(s_inputPath);
//         }
//         var provider = new InputDataDirectProvider(asset.ToJson(), overrideJson);
//         await inputSystem.InitSystem(provider);
//         inputSystem.InitPlayerInput(playerInput, PlayerInputOnActionTriggered);
//         inputSystem.Enable();
//         UpdateKeyUI();
//         
//         foreach (var activeAction in inputSystem.GetActiveActions())
//         {
//             foreach (var currentBinding in inputSystem.GetCurrentBindings(activeAction))
//             {
//                 Debug.Log(currentBinding);
//             }
//         }
//     }
//
//     private void EditKey(string actionName, string bindName = "")
//     {
//         inputSystem.EditInput(actionName, bindName).OnApplyBinding(op =>
//         {
//             switch (op.ConflictType)
//             {
//                 case EKeyConflict.Over:
//                     Debug.Log("改键成功");
//                     break;
//                 case EKeyConflict.ExistKey:
//                     op.Exchange();
//                     Debug.Log("检测到冲突，已交换按键");
//                     break;
//                 case EKeyConflict.SpecialKey:
//                 default:
//                     throw new ArgumentOutOfRangeException();
//             }
//             UpdateKeyUI();
//         })
//         .OnCancel(op =>
//         {
//             Logger.LogInfo(ELogTags.Input,$"操作取消");
//             op.Dispose();
//         });
//     }
//
//     private void UpdateKeyUI()
//     {
//         foreach (var activeAction in inputSystem.GetActiveActions())
//         {
//             switch (activeAction)
//             {
//                 case ActionConfigs.NormalAttack:
//                     txtNormalAttack.text = inputSystem.GetCurrentBinding(activeAction, 0).DisplayKey;
//                     break;
//                 case ActionConfigs.Move:
//                     txtMove_Up.text = inputSystem.GetCurrentBinding(activeAction, 1).DisplayKey;
//                     break;
//             }
//         }
//     }
//     
//     private void EditKeyTest(string actionName, string bindName = "")
//     {
//         Debug.Log($"Editing...");
//         playerInput.actions.Disable();
//         var action = playerInput.actions.FindAction(actionName);
//         if (action == null)
//         {
//             Debug.LogWarning($"action not found: {actionName}");
//             return;
//         }
//
//         var bindingIndex = -1;
//         if (action.type == InputActionType.Button)
//         {
//
//         }
//         else
//         {
//             // 假设 Move 是一个 2D Vector 复合 Action，你想改 "Up" 的绑定
//             // 先找到 Up 绑定的索引
//             bindingIndex = action.bindings.IndexOf(binding => binding.name == "Up");
//         }
//
//         var cancelKey = GetCancelPathForCurrentScheme();
//         var operation = action.PerformInteractiveRebinding(bindingIndex).WithCancelingThrough(cancelKey)
//             .OnComplete(operation =>
//             {
//                 // 改键成功
//                 Debug.Log($"新按键: {action.bindings[bindingIndex == -1 ? 0 : bindingIndex].effectivePath}");
//                 operation.Dispose();
//
//                 // 保存覆盖数据
//                 var json = playerInput.actions.SaveBindingOverridesAsJson();
//                 File.WriteAllText(s_inputPath, json);
//                 playerInput.actions.Enable();
//             }).OnCancel(operation =>
//             {
//                 // 用户取消
//                 operation.Dispose();
//                 Debug.Log($"用户取消");
//                 playerInput.actions.Enable();
//             }).Start();
//     }
//     
//     private void PlayerInputOnActionTriggered(InputAction.CallbackContext context)
//     {
//         if (context.action.phase == InputActionPhase.Performed)
//         {
//             Debug.Log($"triggered: {context.action.name}，bindings: {string.Join(',', context.action.bindings)}");
//         }
//     }
//
//     private string GetCancelPathForCurrentScheme()
//     {
//         // 如果没有设置 PlayerInput 或没有活跃方案，就返回键鼠默认
//         if (!playerInput || playerInput.currentControlScheme == null)
//             return "<Keyboard>/escape";
//
//         return playerInput.currentControlScheme switch
//         {
//             "Keyboard&Mouse" => "<Keyboard>/escape",
//             "Gamepad" => "<Gamepad>/buttonEast" // B 键
//             ,
//             _ => "<Keyboard>/escape"
//         };
//     }
// }
//
// #endif