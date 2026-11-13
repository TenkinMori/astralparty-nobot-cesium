# 缺人时不补人机（CesiumLoader 版）

<p align="left">
  <img src="icon.png" width="96" alt="icon">
</p>

> **插件名：缺人时不补人机** ｜ **作者：TenkinMori**
>
> 这是一个 CesiumLoader 托管模组：把 `NoBotMod.dll` 放进
> `AstralParty_ModLoader/mods/NoBotMod/` 就生效。

## 作用

**在自定义房间缺人时不自动补充人机。**

创建房间后点「开始」，客户端会发 `StartGameC2S { IsAddBot = true }`，服务端据此补人机。
本模组在发包前把 `IsAddBot` 强制改成 `false`，于是人不够也直接开局，不补人机。

- 只改本机发出去的那一个字段，不改服务端、不改热更包
- 纯托管实现，不用改游戏文件

## 特点

| | 说明 |
|---|---|
| 载体 | `NoBotMod.dll`（CesiumLoader 托管模组） |
| 依赖 | CesiumLoader 3.x |
| 安装位置 | `AstralParty_ModLoader/mods/NoBotMod/` |
| 游戏更新后 | 只要 `StartGameC2SCall` 签名不变就继续有效 |
| 原理 | 运行时替换 RPC 实例，不发额外包、不写文件 |

## 安装

前提：游戏已安装 CesiumLoader（根目录有 `version.dll` 和 `AstralParty_ModLoader/`）。

1. 关闭游戏
2. 把 `dist/NoBotMod/` 整个文件夹复制到：

   ```text
   游戏目录/AstralParty_ModLoader/mods/NoBotMod/
   ├─ NoBotMod.dll
   ├─ NoBotMod.json
   └─ icon.png
   ```

3. 重启游戏

看 `AstralParty_ModLoader/logs/activity-mod-*.log`，出现下面两行即生效：

```text
[NoBotMod] 已接管 StartGameC2S：IsAddBot 强制 false
[缺人时不补人机] 初始化完成 (状态: Initialized)
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

## 版本

- `1.1.0`：插件名 **缺人时不补人机**、作者 TenkinMori、新图标

## 兼容性

- CesiumLoader SDK 3.1.1+（模组清单声明的下限；实测 3.7.0）
- 游戏版本 3.3.0（2026-11-12 更新后）
- 仅 Windows x64

## 许可

- 本模组代码：MIT（见 `LICENSE`）
- `lib/CesiumLoader.SDK.dll`：CesiumLoader SDK，MIT，版权归其作者
- `refs/game/` 下的游戏程序集版权归游戏官方，仅用于编译，不随本模组运行
