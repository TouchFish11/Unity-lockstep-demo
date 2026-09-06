using System.Threading.Tasks;
using Core.DI;
using Core.GlobalEvent;
using Core.GlobalEvent.Events.Net;
using Core.Inputs;
using Core.Inputs.Providers;
using Core.Mono;
using Core.Net.SyncModule.Interface;
using Core.UI.ViewController;
using UnityEngine;

namespace HotUpdate.UI
{
    public class RaceController : UIController<RaceView>
    {
        [Inject] private INetGameProxy _proxy;
        [Inject] private IEventCenter _eventCenter;
        [Inject] private IMonoAdapter _monoAdapter;
        [Inject] private IInputSystem _inputSystem;

        public Transform View => view.transform;
        
        protected override async Task OnInit()
        {
            // 初始化InputSystem
            await _inputSystem.InitSystem(new InputDataDefaultProvider(AssetKeys.PlayerControls));
            _eventCenter.SubscribeEvent<FrameHandleEvent>(HandleFrameEvent);
            _monoAdapter.AddUpdateListener(OnUpdate);
            _proxy.TcpRtt += OnTcpRtt;
        }

        protected override Task OnActive()
        {
            return Task.CompletedTask;
        }

        private void HandleFrameEvent(FrameHandleEvent frameHandleEvent)
        {
            view.txtFrameIDInfo.text = frameHandleEvent.FrameId.ToString();
        }

        private void OnUpdate()
        {
            var frameRate = Application.targetFrameRate;
            var frameRateText = frameRate != -1 ? frameRate.ToString() : "无限制";
            view.txtFrameRateInfo.text = frameRateText;
        }
        
        private void OnTcpRtt(long rttMs)
        {
            view.txtTcpNetRTTInfo.text = $"{rttMs.ToString()}ms";
        }

        protected override Task OnInactivate()
        {
            _eventCenter.UnsubscribeEvent<FrameHandleEvent>(HandleFrameEvent);
            _monoAdapter.RemoveUpdateListener(OnUpdate);
            _proxy.TcpRtt -= OnTcpRtt;
            return Task.CompletedTask;
        }
    }
}
