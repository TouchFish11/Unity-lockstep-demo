using System;
using Core.DI;
using Core.GlobalEvent;
using Core.Net.Events;
using Core.Net.Protocols;
using Core.Net.Protocols.Tcp.Messages.Battle.C2S;
using Core.Net.SyncModule.Interface;
using Core.UI;
using TMPro;
using UnityEngine.UI;

namespace HotUpdate.UI
{
    public class ConfirmPanelUI : UIBehaviourBase
    {
        [Inject] private INetManager _netManager;
        
        [InjectUI] private TextMeshProUGUI txtConfirmInfo;
        [InjectUI] private Button btnConfirm;
        [InjectUI] private Button btnRefuse;

        private int matchPlayerCount;
        private int confirmedCount;
        
        private Action onPlayerCancel;

        protected override void OnEnable()
        {
            DIContainer.GetInstance<IEventCenter>().SubscribeEvent<ClientConfirmMatchStateEvent>(ClientConfirmMatchStateEvent);
        }

        protected override void OnButtonClick(string btnName)
        {
            switch (btnName)
            {
                case nameof(btnConfirm):
                    Confirm();
                    break;
                case nameof(btnRefuse):
                    Refuse();
                    break;
            }
        }

        public void Init(int matchPlayerCount, Action onPlayerCancel)
        {
            this.matchPlayerCount = matchPlayerCount;
            this.onPlayerCancel = onPlayerCancel;
            UpdatePlayerConfirm(0);
        }
        
        /// <summary>
        /// 接受匹配
        /// </summary>
        private void Confirm()
        {
            _netManager.Send(new C2S_MatchConfirmMessage { IsMatch = true }, EProtocolChannel.Resolve);
            btnConfirm.enabled = false;
            btnRefuse.enabled = false;
        }

        private void Refuse()
        {
            _netManager.Send(new C2S_MatchConfirmMessage { IsMatch = false }, EProtocolChannel.Resolve);
            btnConfirm.enabled = false;
            btnRefuse.enabled = false;
        }

        private void ClientConfirmMatchStateEvent(ClientConfirmMatchStateEvent matchStateEvent)
        {
            if (matchStateEvent.ConfirmMatchState)
            {
                UpdatePlayerConfirm(++confirmedCount);
            }
            else
            {
                onPlayerCancel?.Invoke();
            }
        }

        private void UpdatePlayerConfirm(int currentConfirmedCount)
        {
            txtConfirmInfo.text = $"{currentConfirmedCount}/{matchPlayerCount}";
        }

        protected override void OnDisable()
        {
            DIContainer.GetInstance<IEventCenter>().UnsubscribeEvent<ClientConfirmMatchStateEvent>(ClientConfirmMatchStateEvent);
        }
    }
}
