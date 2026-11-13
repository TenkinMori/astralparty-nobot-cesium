# NoBot（CesiumLoader 版）

> **这是 CesiumLoader 模组版本**：一个托管模组 `NoBotMod.dll`，放进
> `AstralParty_ModLoader/mods/NoBotMod/` 就生效。
>
> 它**不是** [astralparty-nobot](https://github.com/TenkinMori/astralparty-nobot) 那个
> `hid.dll`（原生代理 + 内嵌补丁包）版本——两个版本功能一样，选一个装即可。

## 是什么

创建房间后点「开始」，客户端会发 `StartGameC2S { IsAddBot = true }`，服务端据此补 Bot。
本模组在发包前把 `IsAddBot` 强制改成 `false`，于是**不弹「是否补充 Bot」，直接开局**。

- 只改本机发出去的那一个字段，不改服务端、不改热更包
- 不需要 hid.dll、不需要按客户端版本重打 payload

## 两个版本对比

| | **CesiumLoader 版**（本仓库） | hid.dll 版（astralparty-nobot） |
|---|---|---|
| 载体 | `NoBotMod.dll`（托管模组） | `hid.dll`（原生代理 + 内嵌补丁包） |
| 依赖 | 需要 CesiumLoader 3.x | 无 |
| 安装位置 | `AstralParty_ModLoader/mods/NoBotMod/` | 游戏根目录 |
| 游戏更新后 | 只要 `StartGameC2SCall` 签名不变就继续有效 | 热更包大小变了要重新生成 payload |
| 原理 | 运行时替换 RPC 实例 | 拦截 `CreateFileW`，把热更包换成补丁版 |

## 安装

前提：游戏已安装 CesiumLoader（根目录有 `version.dll` 和 `AstralParty_ModLoader/`）。

1. 关闭游戏
2. 把 `dist/NoBotMod/` 整个文件夹复制到：

   ```text
   游戏目录/AstralParty_ModLoader/mods/NoBotMod/
   ├─ NoBotMod.dll
   └─ NoBotMod.json
   ```

3. 重启游戏

看 `AstralParty_ModLoader/logs/activity-mod-*.log`，出现下面两行即生效：

```text
[NoBotMod] 已接管 StartGameC2S：IsAddBot 强制 false
[开局不补 Bot] 初始化完成 (状态: Initialized)
```

卸载：删除 `mods/NoBotMod/` 文件夹，重启游戏。

## 原理

`Core.Net.StartGameC2SRPC` 是 public、非 sealed，`StartGameC2SCall` 是 public virtual，
`Core.Net.RPCMsgManager.StartGameC2S` 是 public 字段，所以可以直接换实例：

```csharp
internal sealed class NoBotStartGameRPC : StartGameC2SRPC
{
    public override RPCAsyncResult StartGameC2SCall(StartGameC2S req)
    {
        if (req != null) req.IsAddBot = false;
        return base.StartGameC2SCall(req);
    }
}
```

```csharp
NetManager net = MonoSingletonProvider<NetManager>.inst;
net.RPC.StartGameC2S = new NoBotStartGameRPC();
```

重连后 RPC 管理器可能重建，`OnUpdate` 每秒巡检一次，发现不是我们的子类就重新挂上。
全程纯托管：不碰 `AstralParty.Runtime.dll` 补丁、不用 Harmony、不写文件。

## 构建

需要 .NET SDK 8+：

```powershell
pwsh -File build.ps1
```

产物在 `dist/NoBotMod/`。

- `lib/CesiumLoader.SDK.dll`：CesiumLoader SDK（MIT，随仓库提供以便直接构建）
- `refs/game/*.dll`：游戏热更程序集（2026-11-12 国服版本），只用于编译引用；
  游戏更新后可用 CesiumLoader 仓库的 `tools/fetch-game-refs.ps1` 重新提取，或替换本目录

## 兼容性

- CesiumLoader SDK 3.1.1+（模组清单声明的下限；实测 3.7.0）
- 游戏版本 3.3.0（2026-11-12 更新后）
- 仅 Windows x64

## 许可

- 本模组代码：MIT（见 `LICENSE`）
- `lib/CesiumLoader.SDK.dll`：CesiumLoader SDK，MIT，版权归其作者
- `refs/game/` 下的游戏程序集版权归游戏官方，仅用于编译，不随本模组运行
