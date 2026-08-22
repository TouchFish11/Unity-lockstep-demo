using System;
using System.Threading.Tasks;
using Core.DI;
using Core.Log;
using Core.UI;
using HotUpdate.UI.UI;
using UnityEngine;
using Logger = Core.Log.Logger;

namespace HotUpdate.Entry
{
    /// <summary>
    /// 热更新入口
    /// </summary>
    public class HotUpdateEntry : MonoBehaviour
    {
        private IUIManager _uiManager;
        
        private void Awake()
        {
            _uiManager = DIContainer.Resolve<IUIManager>();
        }

        private async void OnEnable()
        {
            try
            {
                // 在OnEnable执行run逻辑，而不是在Start，因为Start执行晚于该对象的释放
                await Run();
            }
            catch (Exception e)
            {
                Logger.LogException(ELogTags.HotUpdateEntry, e);
            }
        }

        /// <summary>
        /// 运行
        /// </summary>
        private async Task Run()
        {
            // 初始化UI管理器，创建画布和UI相机
            await _uiManager.InitUIManagerAsync(AssetKeys.UIRoot);
            // 开始界面
            await _uiManager.CreateViewAsync<MainView, MainController>(AssetKeys.Main_Panel, E_UILayer.Mid);
        }
    }
}
