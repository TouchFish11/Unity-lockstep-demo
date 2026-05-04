using System;
using System.Threading.Tasks;
using Core.AssetBundles.Management;
using Core.DI;
using Core.Log;
using Core.UI.MVC;
using HotUpdate.Base.Tip;
using HotUpdate.UI.Inventory;

namespace HotUpdate.UI.Tip
{
    /// <summary>
    /// 提示界面控制器
    /// </summary>
    public class TipController : UIController<TipView, TipModel>
    {
        [Inject] private ObjectSpawner _objectSpawner;
        
        private event Action _onConfirm;
        
        protected override Task OnInit()
        {
            return Task.CompletedTask;
        }

        protected override Task OnActive()
        {
            return Task.CompletedTask;
        }

        protected override Task OnInactivate()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// 设置提示
        /// </summary>
        /// <param name="confirmData">确认数据</param>
        public async void SetTip(ConfirmData confirmData)
        {
            try
            {
                view.txtTipTitle.text = confirmData.ConfirmTitle;
                _onConfirm = confirmData.OnConfirm;
                var contentUI = await LoadContentUI(confirmData.ConfirmContent);
                contentUI.DrawContent(confirmData.ContentData);
            }
            catch (Exception e)
            {
                Logger.LogError($"[{nameof(TipController)}] create tip content prefab error, {e.Message}");
            }
        }

        /// <summary>
        /// 加载提示内容UI
        /// </summary>
        /// <param name="confirmContent"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private async Task<IConfirmContent> LoadContentUI(EConfirmContent confirmContent)
        {
            switch (confirmContent)
            {
                case EConfirmContent.Delete:
                    var poolObj = await _objectSpawner.SpawnAsync<DeleteItemConfirmContent>(AssetKeys.DeleteItemConfirmContent, view.ContentRoot);
                    model.ConfirmContent = poolObj;
                    DIContainer.InjectIntoInstance(poolObj.Obj);
                    return poolObj.Obj;
                default:
                    throw new ArgumentOutOfRangeException(nameof(confirmContent), confirmContent, null);
            }
        }

        protected override async void OnButtonClick(string btnName)
        {
            if (btnName == nameof(view.btnOk))
            {
                _onConfirm?.Invoke();
            }
            
            _onConfirm = null;
            model.ConfirmContent.Convert<IConfirmContent>().Obj.ClearContent();
            await uiManager.DestroyView(panelId);
        }

        protected override Task OnDestroy()
        {
            _objectSpawner.Dispose();
            return base.OnDestroy();
        }
    }
}
