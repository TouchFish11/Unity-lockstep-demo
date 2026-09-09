using System.Collections.Generic;
using System.Threading.Tasks;
using Core.AssetBundles.Management;
using Core.DI;
using Core.GlobalEvent;
using Core.GlobalEvent.Events.Net;
using Core.Inputs;
using Core.Inputs.Providers;
using Core.Mono;
using Core.Net.Protocols;
using Core.Net.Protocols.Tcp.Messages.Common;
using Core.Net.SyncModule.Interface;
using Core.UI.ViewController;
using HotUpdate.Game.Race.View;
using TMPro;
using UnityEngine;

namespace HotUpdate.UI
{
    public class RaceController : UIController<RaceView>
    {
        [Inject] private INetManager _netManager;
        [Inject] private IEventCenter _eventCenter;
        [Inject] private IMonoAdapter _monoAdapter;
        [Inject] private IInputSystem _inputSystem;
        [Inject] private ObjectSpawner _objectSpawner;

        // 是否已离开比赛
        private bool _isLeave;
        private readonly List<StatusHUD> _huds = new();
        
        protected override async Task OnInit()
        {
            // 初始化InputSystem
            await _inputSystem.InitSystem(new InputDataDefaultProvider(AssetKeys.PlayerControls));
            _eventCenter.SubscribeEvent<FrameHandleEvent>(HandleFrameEvent);
            _monoAdapter.AddUpdateListener(OnUpdate);
            ((IHeartbeatService)_netManager).OnRttCalc += OnTcpRtt;
        }

        protected override Task OnActive()
        {
            return Task.CompletedTask;
        }

        private void HandleFrameEvent(FrameHandleEvent frameHandleEvent)
        {
            view.txtFrameIDInfo.text = frameHandleEvent.FrameId.ToString();
        }

        public async Task CreateHUD(ViewAvatar viewAvatar)
        {
            var statusHUD = await _objectSpawner.SpawnAsync<StatusHUD>(AssetKeys.StatusHUD);
            statusHUD.Init(viewAvatar, view.transform);
            _huds.Add(statusHUD);
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

        protected override void OnButtonClick(string btnName)
        {
            if (btnName == nameof(view.btnLeaveOrReConnect))
            {
                if (!_isLeave)
                {
                    view.btnLeaveOrReConnect.GetComponentInChildren<TextMeshProUGUI>().text = "重新连接";
                    // 发送离开消息
                    _netManager.Send(new C2S_RequestDisConnectMessage(), EProtocolChannel.Resolve);
                }
                else
                {
                    view.btnLeaveOrReConnect.GetComponentInChildren<TextMeshProUGUI>().text = "离开比赛";
                    // 发送重连消息
                    _netManager.Connect();
                }
                
                _isLeave = !_isLeave;
            }
        }

        protected override Task OnInactivate()
        {
            _eventCenter.UnsubscribeEvent<FrameHandleEvent>(HandleFrameEvent);
            _monoAdapter.RemoveUpdateListener(OnUpdate);
            ((IHeartbeatService)_netManager).OnRttCalc -= OnTcpRtt;
            _objectSpawner.Release(_huds);
            _objectSpawner.Clear();
            return Task.CompletedTask;
        }
    }
}
