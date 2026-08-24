using System.Threading.Tasks;
using Core.DI;
using Core.GlobalEvent;
using Core.GlobalEvent.Events.Net;
using Core.Mono;
using Core.Net.SyncModule.Interface;
using Core.UI;
using Core.UI.ViewController;
using UnityEngine;
using UnityEngine.UI;

namespace HotUpdate.UI
{
    public class RaceController : UIController<RaceView>
    {
        [InjectUI] private Text txtTcpNetRTTInfo;
        [InjectUI] private Text txtFrameRateInfo;
        [InjectUI] private Text txtFrameIDInfo;
        [InjectUI] private Text txtRenderPosInfo;
        [InjectUI] private Text txtLogicPosInfo;
        [InjectUI] private Toggle togLeaveOrReConnect;

        [Inject] private INetGameProxy _proxy;
        [Inject] private IEventCenter _eventCenter;
        [Inject] private IMonoAdapter _monoAdapter;
        
        protected override Task OnInit()
        {
            _eventCenter.SubscribeEvent<FrameHandleEvent>(HandleFrameEvent);
            _monoAdapter.AddUpdateListener(OnUpdate);
            _proxy.TcpRtt += OnTcpRtt;
            return Task.CompletedTask;
        }

        protected override Task OnActive()
        {
            return Task.CompletedTask;
        }

        private void HandleFrameEvent(FrameHandleEvent frameHandleEvent)
        {
            txtFrameIDInfo.text = frameHandleEvent.FrameId.ToString();
        }

        private void OnUpdate()
        {
            txtFrameRateInfo.text = Application.targetFrameRate.ToString();
        }
        
        private void OnTcpRtt(long rttMs)
        {
            txtTcpNetRTTInfo.text = $"{rttMs.ToString()}ms";
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
