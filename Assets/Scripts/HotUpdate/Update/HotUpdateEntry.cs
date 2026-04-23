using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.AssetBundles.Management;
using Core.DI;
using Core.Input.ActionAsset;
using Core.Scene;
using Core.UI;
using Core.Utility;
using HotUpdate.Base;
using HotUpdate.Game.Data;
using HotUpdate.Game.Inventory;
using HotUpdate.Game.Main.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Logger = Core.Log.Logger;
using SceneManager = Core.Scene.SceneManager;

namespace HotUpdate.Update
{
    public class HotUpdateEntry : MonoBehaviour
    {
        [Inject] private ObjectSpawner _objectSpawner;
        [Inject] private IInputSystem _inputSystem;
            
        private async void Start()
        {
            try
            {
                await Run();
            }
            catch (Exception e)
            {
                Logger.LogError($"{nameof(HotUpdateEntry)}: hotfix entry error {e.Message}");
            }
        }

        /// <summary>
        /// 运行
        /// </summary>
        private async Task Run()
        {
            try
            {
                // 初始化游戏设置
                await InitSettings();
                // 初始化UI管理器，创建画布和UI相机
                var uiManager = DIContainer.Create<UIManager>();
                await uiManager.InitUIManagerAsync(AssetKeys.UIRoot);
                // 显示开始界面
                var controller = await uiManager.CreateViewAsync<BeginView, BeginModel, BeginController>(AssetKeys.BeginView, E_UILayer.Mid);
                // 进入游戏
                controller.OnClickEnterGame += EnterGame;
                // 检查更新
                controller.CheckUpdate();
            }
            catch (Exception e)
            {
                Logger.LogError($"{nameof(HotUpdateEntry)}:Error occurred while running the hot update entry,{e.Message}");
            }
        }

        /// <summary>
        /// 初始化设置
        /// </summary>
        private static Task InitSettings()
        {
            // ...
            return Task.CompletedTask;
        }
        
        /// <summary>
        /// 进入游戏
        /// </summary>
        private async Task EnterGame()
        {
            try
            {
                var tasks = new List<Task>
                {
                    // 初始化管理器
                    InitManagers(),
                    // 切换场景
                    LoadSceneAsync(),
                    // 初始化输入系统
                    InitInputSystemAsync(),
                    // 初始化游戏数据
                    LoadPlayerDataAsync()
                };
                
                await Task.WhenAll(tasks);
                
                // 打开主界面
                await DIContainer.Create<UIManager>().CreateViewAsync<MainPanel, MainModel, MainController>(AssetKeys.Main_Panel,  E_UILayer.Mid);
            }
            catch (Exception e)
            {
                Logger.LogError($"{nameof(HotUpdateEntry)}:Entry game error,{e.Message}");
            }
        }

        public static Task InitManagers()
        {
            // 绑定背包管理器类型
            DIContainer.BindSingleton<IInventoryManager, InventoryManager>();
            //...
            
            return Task.CompletedTask;
        }
        
        /// <summary>
        /// 初始化输入系统
        /// </summary>
        private async Task InitInputSystemAsync()
        {
            Logger.Log($"{nameof(HotUpdateEntry)}:Initialization of the InputSystem is complete");
            return;
            var handle = await GameAsset.LoadAssetAsync<TextAsset>(FileUtility.InputActionLocalFileName);
            _inputSystem.InitInputSystem(handle.Asset.text);
            GameAsset.Release(handle);
            Logger.Log($"{nameof(HotUpdateEntry)}:Initialization of the InputSystem is complete");
        }

        /// <summary>
        /// 异步加载玩家数据
        /// </summary>
        private static async Task LoadPlayerDataAsync()
        {
            var gameDataManager = DIContainer.Create<GameDataManager>(true);
            await Task.WhenAll(gameDataManager.LoadConfigAsync(),  gameDataManager.LoadDataAsync());
            Logger.Log($"{nameof(HotUpdateEntry)}:Initialization of the GameData is complete");
        }

        /// <summary>
        /// 加载并切换场景
        /// </summary>
        /// <returns></returns>
        private static Task LoadSceneAsync()
        {
            return DIContainer.Create<SceneManager>().LoadSceneAsync(AssetKeys.InventoryTestScene, LoadSceneMode.Single, null);
        }
    }
}
