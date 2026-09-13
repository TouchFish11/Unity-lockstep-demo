# Unity-Lockstep-Demo — Unity 帧同步（Lockstep）战斗 Demo

一个**专门用于实践帧同步（Lockstep）**的项目，由 **Unity 客户端 + 服务器**两端协同。它在同一套自研客户端框架（HybridCLR 热更新、DI、事件中心、AssetBundle、MVC/MVVM UI 等）之上，
实现了一条完整的**确定性帧同步链路**：定点数数学库 → 帧同步协议 → 服务器定帧广播 → 客户端帧循环（缓冲/对齐/纠偏/补帧/追帧）→ 逻辑与表现分离 → 断线重连，
并在其上落地了一个"玩家 + 确定性 AI"的 PVE 战斗玩法（普攻/AOE 技能、扇形与圆形命中、碰撞分离、胜负判定），以及**帧输入录制回放**。

> 开发环境：Unity 2022.3 LTS（2022.3.57f1c2）｜ 独立开发

---

## 🎯 帧同步核心（本项目重点）

锁步同步要求**所有客户端在同一逻辑帧上，用相同的输入得到相同的结果**。本项目围绕"确定性"层层展开：

### 1. 确定性数学基础（`Core/Math`）

| 组件 | 做了什么 | 为什么这么做 |
| --- | --- | --- |
| **`Fixed64` 定点数** | 16 位小数（Q16.16）的定点运算，支持 +−×÷、开方、绝对值 | 用整数模拟小数，规避浮点跨平台不一致，是锁步确定性的根基 |
| **`FixedVector3`** | 定点三维向量：点积、叉积、长度、归一化、距离 | 逻辑层的位移/朝向/判定全部使用定点向量，杜绝 `Vector3` 浮点误差 |
| **`DeterministicRandom`** | xorshift32 伪随机数（固定种子），`NextFixed64`/`Range` 输出定点数 | 逻辑层禁止 `UnityEngine.Random`，保证随机序列在所有客户端一致 |

### 2. 帧同步协议（`Core/Net/Protocols/FSync`）

| 类型 | 职责 |
| --- | --- |
| **`OptMessage`** | 一帧内的单条操作：`RaceID + OptType + Arg1/2/3`（通用参数复用） |
| **`EOptType`** | 操作类型枚举：`None / Move / Attack / UseSkill` |
| **`InputCommand` + `CommandCodec`** | 输入命令结构与编解码：把逻辑层命令序列化为 `OptMessage`，再反解回来 |
| **`S2C_FrameMessage`** | 服务器下发的**一包多帧**：`List<S2C_OneFrameMessage>` |
| **`S2C_OneFrameMessage`** | 某一帧 ID 下所有客户端的操作集合 |
| **`C2S_NextFrameMessage`** | 客户端上行："第 N 帧我的操作" |
| **`C2S_RequestFramesMessage`** | 客户端请求从某帧开始**补发**（断线重连/丢帧追帧） |

### 3. 客户端帧循环（`S2C_FrameMessageHandler`）

帧同步在客户端侧的"心脏"，核心流程如下：

- **固定逻辑帧**：逻辑帧时长 `66ms`（`LogicTime = 0.066f`），由 `accumulator` 累加驱动。
- **输入提前量**：`preSendInput = 1`，每帧采样输入后发送"本地帧号 + 1"帧的输入，提前量用于对抗网络延迟。
- **帧缓冲 + 按序执行**：收到的帧先放入 `SortedDictionary<int, S2C_OneFrameMessage>`，从"已执行帧 + 1"开始，只要缓冲里有就按顺序取出执行，**乱序到达也能保证执行顺序一致**。
- **对齐与纠偏**：首次收到帧时把本地帧号锚定到服务器帧号；本地 tick 落后于服务器（时钟漂移）时，拉到服务器帧号后面。
- **缺口检测与补帧**：执行完后缓冲里仍残留帧，说明中间缺帧，发送 `C2S_RequestFramesMessage` 请求补发；同一缺口只请求一次。
- **追帧**：补帧期间每执行一帧 `await Task.Yield()`，让渲染帧有机会刷新，避免一次性追帧卡死主线程。
- **断线重连**：`ReconnectToRace()` 先发送重连认领消息（新连接 ID + 稳定 RaceId），再请求补发漏掉的帧。

### 4. 传输层（`Core/Net`）

| 模块 | 做了什么 |
| --- | --- |
| **`NetManager`** | 统一网络管理器：支持 `TCP / KCP / Dual` 三种客户端（`EClientType`，默认 KCP），收包后交给 `MessageRouter` 分发 |
| **TCP 分包粘包** | `TcpClient` 以"消息头 [ID+长度]（8 字节）"解析，够头够体才解析、分包回退索引等待 |
| **kcp2k** | 集成 KCP 可靠 UDP 传输实现，适合低延迟帧同步场景 |
| **`MessageRouter` / `MessageFactory`** | 消息 ID ↔ 消息类型路由与工厂，反序列化后分发到对应 Handler |
| **`IHeartbeatService`** | 心跳保活 + RTT 计算（`OnRttCalc` 回调），UI 实时显示 TCP RTT |
| **`NetGameManager`** | 维护"比赛 ID（RaceId）↔ 玩家对象"，并把 RaceId 持久化到本地 JSON，**用于断线重连认领身份** |

### 5. 服务器端

| 模块 | 做了什么 |
| --- | --- |
| **`ServerManager`** | 服务器管理器：支持 `TCP / KCP` 双通道（`EServerType`），主循环每 66ms tick 驱动帧广播 |
| **`RaceHandleComponent`** | 帧同步核心：`_serverFrame` 定帧号，`_inputCache` 按帧缓存输入（迟到帧丢弃），`BroadcastFrame` 广播当前帧；`_history` 缓存历史帧供补发 |
| **补发** | `HandleRequestFrames` 从 `StartFrame` 补到当前帧，响应客户端追帧请求 |
| **匹配流程** | 加入匹配 → 匹配成功 → 等待确认 → 准备比赛 → 等待就绪 → 开始比赛 → 比赛结束（状态轮询驱动） |
| **断线重连** | `MarkWaitReconnect` 断开时保留比赛身份，`Reconnect` 重连后更新连接 ID |
| **调试** | `SkipFrameId` 可跳过广播指定帧，专用于测试追帧补发 |

### 6. 逻辑 / 表现 / 视图分离

| 层 | 位置 | 职责 |
| --- | --- | --- |
| **逻辑层** | `HotUpdate/Game/Race/Logic` | `LogicWorld` / `LogicAvatar` / `AiController`，纯 C# 确定性逻辑，无 `MonoBehaviour` |
| **表现事件** | `HotUpdate/Game/Race/Present` | `PresentEvent` / `PresentEventsEvent`（特效/音效等表现指令），逻辑层只"声明"表现，不直接操作渲染 |
| **视图层** | `HotUpdate/Game/Race/View` | `ViewAvatar : MonoBehaviour`，把逻辑位置**插值**到渲染位置，播放动画、采集输入 |

逻辑层是确定性的（同输入同结果），视图层只负责"把逻辑结果好看地展示出来"，两者通过版本号（`Version`）与插值（`PrevPosition → Position`）解耦。
命中特效链路：逻辑层 `AddEffect` 收集表现事件 → 帧末 `FlushEffects` 批量发 `PresentEventsEvent` → 表现层（`RaceContext` / `ReplayContext`）按 `EffectId` 生成 `VfxTimer` 特效。

---

## ⚔️ 确定性战斗逻辑（`LogicWorld` / `LogicAvatar`）

每逻辑帧 `LogicWorld.Tick()` 严格按固定顺序推进，保证确定性：

1. **玩家命令**：按 `raceId` 找到对应 `LogicAvatar` 执行命令（移动 / 普攻 / 技能）。
2. **AI**：固定序遍历 `AiController`，确定性 AI 找最近存活玩家，距离内转向释放技能、否则归一化方向追击。
3. **技能状态机**：`StartAbility` 检查死亡/施法中/冷却后进入施法，`AbilityConfig` 配置 `Duration`（时长）/`HitFrame`（命中帧）/`Cooldown`（冷却）。
4. **命中结算**：处于命中帧的攻击者，按技能 `Shape` 判定命中 —— **扇形**（`Fan`：朝向点积 ≥ `HalfAngleCos`）或**圆形**（`Circle`：自身范围必中），叠加距离、阵营、死者过滤后扣血。
5. **碰撞分离**：`3` 次迭代两两推开（各推一半），完全重合时沿固定轴推开避免除零；死者不参与碰撞。
6. **胜负判定**：怪物全死 = 胜利，玩家全死 = 失败，只触发一次，发出 `RaceEndEvent`。

**配置驱动（`CharacterTable` / `AbilityTable`）**：

| 配置 | 内容 |
| --- | --- |
| **角色（CharacterConfig）** | 玩家 `maxHp=100 / speed=3 / radius=0.5`，怪物 `maxHp=150 / speed=2 / radius=0.5` |
| **普攻（Attack）** | 扇形，`Range=2 / HalfAngleCos=0.5(120°) / Damage=5 / Duration=8 / HitFrame=3` |
| **回旋斩（AoeSkill）** | 圆形 AoE，`Range=3 / Damage=20 / Cooldown=30 / HitFrame=3` |

> 关键约束：施法中锁移动、死亡锁行动；移动位置版本号 `Version++`，碰撞 `Nudge` 只平移 Position 不改版本号，保证插值目标始终跟随最终逻辑位置。

---

## 🎬 回放系统（`ReplayRecorder` / `ReplayContext`）

帧同步天然支持回放——只要把每帧输入录下来，就能离线确定性重演：

- **录制**：`ReplayRecorder` 在每逻辑帧 `Tick` 时 `Record` 快照拷贝该帧的 `List<InputCommand>`。
- **回放**：`ReplayContext` 重建逻辑世界与表现层，按 66ms 重新 `_logicWorld.Tick(ReplayRecorder.Frames[_frameIndex++])` 逐帧重演，命中特效同样还原。
- **入口**：主界面 `btnReplay` 按钮（`HasRecording()` 判断是否有录像）。

---

## 🧱 框架层（`Assets/Scripts/Core`，复用自研客户端框架）

这套框架按"框架（不热更）+ 业务（热更）"分层，核心模块如下：

| 模块 | 做了什么 | 为什么这么做 |
| --- | --- | --- |
| **热更新（HybridCLR）** | `GameLauncher` 加载 hotfix DLL 与 AOT 元数据，`HotUpdateEntry` 为热更入口；业务拆到 `HotUpdate.Base / Common / Game / UI / Update` | 框架层保持稳定，只热更业务，降低热更风险 |
| **DI 依赖注入** | 手写 `DIContainer`：单例/瞬态、`[Inject]` 注入、`As<T>()` 多接口绑定 | 深入理解注入原理，不依赖 Zenject/VContainer |
| **事件中心** | `EventCenter` 订阅/取消/同步触发/延迟触发，按类型过滤 | 单帧触发上限 + 递归深度保护，防止事件风暴 |
| **资源管理** | `AssetBundleManager` 句柄化生命周期 + 滑动窗口缓存，`GameAsset` 统一加载入口 | 引用计数管理生命周期，避免泄漏与重复加载 |
| **AB 打包与更新** | 编辑器一键打包；运行时 `AssetBundleUpdater` 状态机（校验→下载目录→对比→下载→完整性校验），HTTP 断点续传 | 每步独立可重试/回退，异常可恢复 |
| **网络层** | TCP/KCP/Dual 三通道 + 帧同步协议 + 消息路由 + 心跳 RTT | 见上文"帧同步核心" |
| **UI 框架** | `UIManager` 分层（`E_UILayer`），MVC（`UIController`/`UIView`）+ MVVM（`ReactiveProperty<T>`），反射控件绑定（`[InjectUI]`） | 数据驱动，数据与 UI 解耦 |
| **输入系统** | `InputSystem` + 数据源 Provider（默认/直连/路径），`ActionConfigs` 代码生成 | 输入与逻辑解耦，可切换输入源 |
| **序列化** | JSON（`JsonManager`）+ 二进制（`BinaryDataManager`）+ 自定义消息序列化 | 满足配置、存档、网络消息不同场景 |
| **对象池 / 单例 / 日志** | `PoolManager`（对象池+数据池）、单例基类、分级标签日志 | 减少 GC 压力；统一日志便于定位 |
| **Mono 适配 / 异步 / 时间** | `MonoAdapter` 代理生命周期、`AoTask` 异步封装、`TimerManager` 定时器 | 核心逻辑脱离 MonoBehaviour，顺序可读 |
| **编辑器工具链** | Excel 导表、代码生成（Class/Enum/InputAction/AB Key）、AB 打包窗口、HybridCLR 构建工具 | 配置驱动，改表免改代码 |

> 第三方依赖：热更新使用 HybridCLR，JSON 使用 Newtonsoft.Json，KCP 使用 kcp2k，其余核心框架逻辑为手写。

---

## 📦 业务系统

- **匹配 / 大厅**：`MainController` / `MainView` —— 连接/断开服务器、开始/取消匹配、在线玩家列表、匹配成功确认面板。
- **比赛生命周期**：`RaceContext` 封装一场比赛的完整生命周期（`Prepare → Start → Reconnect → Leave`），统一构建逻辑世界 + 玩家/AI 表现 + HUD + 特效，并处理比赛结束清理与回放录制。
- **断线重连**：客户端 `NetGameManager` 本地持久化 RaceId，服务器 `RaceHandleComponent` 保留比赛身份；重连后重建场景并追帧。
- **回放**：`ReplayRecorder` 录制帧输入，`ReplayContext` 离线重演。
- **聊天**：`ChatMessage` / `ChatUI` / `FriendUI` 已搭框架，Handler 暂为占位（未完整接入）。
- **HUD / 调试信息**：`StatusHUD` 血条跟随，`RaceView` 实时显示帧 ID、帧率、TCP RTT。

---

## 🚀 运行流程

1. **启动服务器**：运行 `FrameSyncServer/Server`（.NET 8 控制台），输入 `Start` 启动（KCP）；
2. **启动客户端**：`GameLauncher.Start()` 注册核心框架，读取 `BootConfig.json`，通过 HybridCLR 加载热更程序集，实例化 `HotUpdateEntry`；
3. **进入大厅**：初始化 UI 管理器，打开 `MainView`，连接服务器 / 开始匹配；
4. **匹配成功**：服务器 `RaceHandleComponent` 走"匹配 → 确认 → 准备 → 就绪"流程，客户端 `RaceContext.PrepareAsync` 构建逻辑世界与表现，回 `C2S_ReadyMessage`；
5. **开始比赛**：服务器按 66ms 固定 tick 定帧广播，客户端采样输入 → 上行 → 收帧 → 逐帧执行逻辑 → 表现层插值渲染；
6. **比赛结束 / 重连 / 回放**：`RaceEndEvent` 返回大厅；断线走"认领身份 + 补帧追帧"恢复现场；主界面可一键回放本局。

---

## 📁 目录结构（节选）

**客户端（Unity，本仓库）：**

```
Assets/
├── Scripts/
│   ├── Core/                 # 框架层（不热更）
│   │   ├── Math/             #   定点数 Fixed64 / FixedVector3 / DeterministicRandom
│   │   ├── Net/              #   网络：kcp2k、协议、帧同步（FSync）、消息路由、心跳
│   │   ├── AssetBundles/     #   AB 打包与更新（状态机 + 断点续传）
│   │   ├── DI/               #   手写依赖注入容器
│   │   ├── GlobalEvent/      #   事件中心
│   │   ├── UI/               #   MVC/MVVM UI 框架
│   │   ├── Inputs/           #   输入系统（Provider + 代码生成）
│   │   └── ...               #   对象池、单例、日志、Mono 适配、序列化、时间等
│   ├── Game/                 # 启动器 GameLauncher（AOT）+ 输入/移动/Vfx 等 Unity 侧实现
│   └── HotUpdate/            # 业务层（热更程序集）
│       ├── Base/             #   Icon 等基础
│       ├── Common/           #   配置数据（Excel 生成）、工具
│       ├── Game/             #   比赛（Race：Logic/Present/View + 配置表/回放）、大厅、聊天
│       ├── UI/               #   各系统界面（MVC/MVVM）：Main、Race、Replay、Loading 等
│       └── Update/           #   热更入口 HotUpdateEntry
├── Editor/                   # 编辑器工具（导表、代码生成、AB 打包、HybridCLR、美术资源配置）
├── ServerData/               # AB 包与 BootConfig 服务器资源
└── ...
```

**服务器：**

```
Server/
├── Program.cs                # 控制台入口（Start/Close/QuitRace 命令）
├── ServerManager.cs          # 服务器管理器（TCP/KCP 双通道 + 66ms 主循环）
├── Clients/                  # 客户端连接封装
├── Game/                     # RaceObject（比赛对象/身份）
├── ProtocolChannel/          # IServer / TcpServer / KcpServer 传输抽象
├── Protocols/                # 消息协议 + 序列化 + 路由（与客户端对称）
│   └── FSync/                #   RaceHandleComponent（定帧/广播/补发/重连）
├── kcp2k/                    # KCP 传输库
```
