using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.DI;
using Core.Loader.Object;
using Core.Singleton;
using Core.UI.MVC;
using UnityEngine;
using Logger = Core.Log.Logger;
using Object = UnityEngine.Object;

namespace Core.UI
{
    /// <summary>
    /// UI管理器
    /// </summary>
    public class UIManager : IUIManager, IInitializable
    {
        public  int InitPriority => 2;
        // 存储打开的界面
        private readonly List<IPanelInfo> _panels = new();
        // 上层
        private Transform _topLayer;
        // 中层
        private Transform _midLayer;
        // 底层
        private Transform _botLayer;
        // 系统层
        private Transform _systemLayer;
        // 预制体加载器对象
        private IPrefabLoader _prefabLoader;
        
        private UIManager()
        {
            
        }

        public Task InitAsync()
        {
            // 要先初始化工厂才能拿到加载器实例
            _prefabLoader = DIContainer.GetInstance<IPrefabLoader>();
            return Task.CompletedTask;
        }

        public async Task InitUIManagerAsync(string defaultAbName, string canvasName, string uiCameraName)
        {
            // 创建画布实例
            Canvas = await DIContainer.GetInstance<IPrefabLoader>().GetObjectAsync<Canvas>(defaultAbName, canvasName, null);
            Object.DontDestroyOnLoad(Canvas.gameObject);

            // 获取对应层级对象位置
            _topLayer = Canvas.transform.Find("Top");
            _midLayer = Canvas.transform.Find("Mid");
            _botLayer = Canvas.transform.Find("Bot");   
            _systemLayer = Canvas.transform.Find("System");
            
            // 创建UI相机实例
            UICamera = await DIContainer.GetInstance<IPrefabLoader>().GetObjectAsync<Camera>(defaultAbName, uiCameraName, null);
            Object.DontDestroyOnLoad(UICamera.gameObject);
            // 设置UI摄像机
            Canvas.worldCamera = UICamera;
        }
        
        public Transform GetLayer(E_UILayer layer)
        {
            return layer switch
            {
                E_UILayer.Top => _topLayer,
                E_UILayer.Mid => _midLayer,
                E_UILayer.Bot => _botLayer,
                E_UILayer.System => _systemLayer,
                _ => null,
            };
        }
        
        public async Task<TController> CreateViewAsync<TView, TModel, TController>(string abName, E_UILayer layer, string panelName, Vector2 pos = default, Quaternion quaternion = default)
            where TView : UIBehaviourBase, IuiView where TModel : IuiModel, new() where TController : class, IuiController, new()
        {
            // 初始化控制器
            var controller = new TController();
            var model = new TModel();
            try
            {
                // 获取面板
                var view = await DIContainer.GetInstance<IPrefabLoader>().GetObjectAsync<TView>(abName, panelName, GetLayer(layer), pos, quaternion);
                await controller.Init(view, model);
                await controller.Show();
                // 初始化面板信息
                var newInfo = new PanelInfo<TView, TModel, TController>(view, model, controller);
                // 存储面板信息
                _panels.Add(newInfo);
                return controller;
            }
            catch (Exception e)
            {
                Logger.LogError($"{nameof(UIManager)}.{nameof(CreateViewAsync)}：异步创建界面错误，{e.Message}");
                return controller;
            }
        }
        
        public async void DestroyView(string abName, IuiController controller)
        {
            try
            {
                for (var i = _panels.Count - 1; i >= 0; i--)
                {
                    var uiController = _panels[i].UiController;
                    if (uiController != controller)
                    {
                        continue;
                    }
                
                    // 调用控制器的销毁
                    await uiController.Destroy();
                    DIContainer.GetInstance<IPrefabLoader>().CollectAsset(_panels[i].UiView.ViewObj);
                    // 释放该UI的资源
                    DIContainer.GetInstance<IPrefabLoader>().RealseAsset(abName, _panels[i].UiView.ViewObj.name);
                    // 从缓存中移除
                    _panels.RemoveAt(i);
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"{nameof(UIManager)}.{nameof(GetController)}：{e.Message}，{e.StackTrace}");
            }
        }
        
        public async Task SetViewActive(IuiController controller, bool isActive)
        {
            if (_panels.ConvertAll(info => info.UiController).Contains(controller))
            {
                if (!isActive)
                {
                    await controller.Hide();
                }
                else
                {
                    await controller.Show();
                }
            }
        }

        public TController GetController<TController>() where TController : IuiController
        {
            foreach (var basePanelInfo in _panels)
            {
                if (basePanelInfo.UiController is TController controller)
                {
                    return controller;
                }
            }
            Logger.LogError($"{nameof(UIManager)}.{nameof(GetController)}：控制器{typeof(TController)}未找到");
            return default;
        }
        
        public void Clear(string abName)
        {
            // 销毁画布和摄像机
            DIContainer.GetInstance<IPrefabLoader>().CollectAsset(Canvas.gameObject);
            DIContainer.GetInstance<IPrefabLoader>().RealseAsset(abName, Canvas.name);
            Canvas = null;
            
            DIContainer.GetInstance<IPrefabLoader>().CollectAsset(UICamera.gameObject);
            DIContainer.GetInstance<IPrefabLoader>().RealseAsset(abName, UICamera.name);
            UICamera = null;
            
            // 销毁所有界面
            foreach (var panelInfo in _panels)
            {
                DestroyView(abName, panelInfo.UiController);
            }
            _panels.Clear();
        }
        
        public List<IPanelInfo> AllPanels => _panels;

        public Canvas Canvas { get; private set; }

        public Camera UICamera { get; private set; }
    }
}
