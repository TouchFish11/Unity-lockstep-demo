using System.Collections.Generic;
using System.Threading.Tasks;
using Core.DI;
using Core.GlobalEvent;
using Core.Mono;
using Core.Net.Events;
using Core.Net.Protocols.FSync.Messages;
using Core.Net.Protocols.Tcp.Messages.Common;
using Core.Net.SyncModule.Interface;
using Core.Net.SyncModule.Manager;
using Core.Time;

namespace Core.Net.Protocols.FSync.Handlers
{
    /// <summary>
    /// 帧消息处理器，处理接收到的服务器帧消息
    /// </summary>
    public class S2C_FrameMessageHandler : MessageHandler<S2C_FrameMessage>
    {
        [Inject] private NetGameManager _netGameManager;
        private readonly INetManager _netManager;
        
        // 逻辑帧时间
        private const float LogicTime = 0.066f;
        // 输入发送提前量
        private const int preSendInput = 1;
        // 本地逻辑帧号，可以理解为是发送的下一个帧的帧号，稳定等于服务器帧ID + 1
        private int _localFrame;         
        // 帧缓冲：帧ID -> 一帧所有输入。TCP/KCP 是有序到达，这里主要做「暂存 + 按序取」
        private readonly SortedDictionary<int, S2C_OneFrameMessage> _frameBuffer = new();
        // 是否已对齐到服务器第一帧
        private bool _isAligned;
        // 帧累加器
        private float accumulator;
        // 上次请求补发的起始帧，避免同一缺口重复请求
        private int _lastRequestedFrame = -1;
        // 已经执行到的帧号。初始 -1，让第 0 帧能被正确执行。
        public int _frameId = -1;
        // 是否正在追帧
        private bool _chasingFrame;
        
        public override S2C_FrameMessage Message { get; protected set; }
        
        private S2C_FrameMessageHandler(IEventCenter eventCenter, IMonoAdapter monoAdapter, INetManager netManager)
        {
            eventCenter.SubscribeEvent<StartRaceEvent>(OnStartRace);
            eventCenter.SubscribeEvent<RequestReconnectRaceEvent>(OnRequestReconnectRace);
            monoAdapter.AddUpdateListener(OnUpdate);
            _netManager = netManager;
        }
        
        protected override void OnHandle()
        {
            InputServerCommand(Message);
        }

        private void OnStartRace(StartRaceEvent startRaceEvent)
        {
            _frameId = -1;
            _frameBuffer.Clear();
            _isAligned = false;
            accumulator = 0;
        }

        private void OnRequestReconnectRace(RequestReconnectRaceEvent requestReconnectRaceEvent)
        {
            ReconnectToRace();
        }

        private void OnUpdate()
        {
            if(!_isAligned)
                return;
        
            accumulator += TimeUtil.DeltaTime;
            if (accumulator >= LogicTime)
            {
                accumulator -= LogicTime;
                // 采样输入，发「第 localFrame + K 帧」的输入
                SendFrameInput(_localFrame + preSendInput);
                _localFrame++;
            }
        }
        
        /// <summary>
        /// 采集本地输入并发送
        /// </summary>
        private void SendFrameInput(int targetFrameId)
        {
            var cmd = new InputCommand { raceId = _netGameManager.RaceId.Value };
            if (_netGameManager.TryGetPlayer(_netGameManager.RaceId.Value, out var netObject))
            {
                netObject.CollectInput(ref cmd);
                var c2SNextFrameMessage = new C2S_NextFrameMessage
                {
                    FrameID = targetFrameId,
                    OptMessage = CommandCodec.Encode(_netGameManager.RaceId.Value, cmd)
                };
                _netManager.Send(c2SNextFrameMessage, EProtocolChannel.Resolve);
            }
        }
        
        /// <summary>
        /// 收到服务器广播的帧数据
        /// </summary>
        public async void InputServerCommand(S2C_FrameMessage s2CFrameMessage)
        {
            // 把收到的所有帧放进缓冲
            foreach (var frame in s2CFrameMessage.FrameMessages)
            {
                _frameBuffer[frame.FrameID] = frame;
            }
            
            // 从「已执行帧 + 1」开始，只要缓冲里有，就按顺序执行
            while (_frameBuffer.TryGetValue(_frameId + 1, out var nextFrame))
            {
                _frameBuffer.Remove(_frameId + 1);
                ExecuteFrame(nextFrame);
                ++_frameId;
                var frameHandleEvent = EventSource.Get<FrameHandleEvent>();
                frameHandleEvent.FrameId = _frameId;
                eventCenter.TriggerEvent(frameHandleEvent);
                
                if (_chasingFrame)
                {
                    await Task.Yield();
                }
            }
            
            // 首次收到帧：把本地帧号锚到服务器帧号，并立刻发第 FrameId+K 帧
            if (!_isAligned)
            {
                _isAligned = true;
                _localFrame = _frameId;
                accumulator = 0;
                SendFrameInput(_localFrame + preSendInput);
                _localFrame++;
            }
            // 逐帧前向纠偏：本地 tick 落后于服务器（时钟漂移）就拉到服务器帧号后面
            else if (_localFrame <= _frameId)
            {
                _localFrame = _frameId + 1;
                accumulator = 0;    // 为什么要置0？？？
            }
            
            // 缺口检测：缓冲里还有帧，说明 FrameId+1 缺失，请求补发
            if (_isAligned && _frameBuffer.Count > 0)
            {
                RequestMissingFrames();
            }
            else
            {
                _chasingFrame = false;
            }
        }
        
        /// <summary>
        /// 检测到缺口，向服务器请求补发从 FrameId+1 开始的帧
        /// </summary>
        private void RequestMissingFrames()
        {
            var startFrame = _frameId + 1;
            // 同一个缺口只请求一次，避免刷屏
            if (startFrame <= _lastRequestedFrame)
                return;
            _lastRequestedFrame = startFrame;

            var msg = new C2S_RequestFramesMessage
            {
                SessionID = _netManager.SessionId,
                StartFrame = startFrame
            };
            _netManager.Send(msg, EProtocolChannel.Resolve);
            _chasingFrame = true;
        }
        
        /// <summary>
        /// 执行帧
        /// </summary>
        /// <param name="frame"></param>
        private void ExecuteFrame(S2C_OneFrameMessage frame)
        {
            var commands = new List<InputCommand>(frame.OptMessages.Count);
            foreach (var optMessage in frame.OptMessages)
            {
                commands.Add(CommandCodec.Decode(optMessage));
            }

            var evt = EventSource.Get<FrameCommandsEvent>();
            evt.Commands = commands;
            eventCenter.TriggerEvent(evt);
        }
        
        /// <summary>
        /// 断线重连：先认领旧身份，再请求补发漏掉的帧
        /// </summary>
        public void ReconnectToRace()
        {
            // 发重连认领消息（SessionID 自动填新连接ID，RaceId 填稳定身份）
            var reconnectMsg = new ReconnectRaceMessage { RaceId = _netGameManager.RaceId.Value };
            _netManager.Send(reconnectMsg, EProtocolChannel.Resolve);

            _localFrame = _frameId;
            accumulator = 0;
            
            // 重置追帧游标，请求补发漏掉的帧
            _lastRequestedFrame = -1;
            RequestMissingFrames();
        }
    }
}
