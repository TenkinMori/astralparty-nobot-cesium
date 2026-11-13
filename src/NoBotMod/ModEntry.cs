using System;
using CesiumLoader.SDK.Mods;
using Core.Net;
using party.protocol;
using Tools;

namespace NoBotMod
{
    public static class ModEntry
    {
        public static void Main()
        {
            ModBase.Run(new NoBotController());
        }
    }

    /// <summary>
    /// 把 <c>NetManager.RPC.StartGameC2S</c> 换成覆盖了 <c>StartGameC2SCall</c> 的子类，
    /// 发包前把 <c>IsAddBot</c> 改成 false。纯托管，不改热更包，也不需要 Harmony。
    ///
    /// 重连/RPC 管理器重建后会换成新实例，所以 <see cref="OnUpdate"/> 每秒巡检补挂一次；
    /// 已经是我们的子类时直接返回，开销可忽略。
    /// </summary>
    public sealed class NoBotController : ModBase
    {
        private const double EnsureIntervalSeconds = 1.0;

        private double _nextEnsure;
        private bool _logged;

        public override string Name { get { return "缺人时不补人机"; } }
        public override string Version { get { return "1.1.0"; } }
        public override string Author { get { return "TenkinMori"; } }
        public override string Description
        {
            get { return "在自定义房间缺人时不自动补充人机，直接开局。"; }
        }

        public override void OnInitialize()
        {
            TryInstall();
        }

        public override void OnUpdate()
        {
            double now = DateTime.UtcNow.TimeOfDay.TotalSeconds;
            if (now < _nextEnsure) return;
            _nextEnsure = now + EnsureIntervalSeconds;
            TryInstall();
        }

        private void TryInstall()
        {
            try
            {
                NetManager net = MonoSingletonProvider<NetManager>.inst;
                if (net == null || net.RPC == null) return;
                if (net.RPC.StartGameC2S is NoBotStartGameRPC) return;

                net.RPC.StartGameC2S = new NoBotStartGameRPC();
                if (!_logged)
                {
                    _logged = true;
                    Log.Info("已接管 StartGameC2S：IsAddBot 强制 false");
                }
            }
            catch (Exception e)
            {
                Log.Warn("安装失败: " + e.Message);
            }
        }
    }

    internal sealed class NoBotStartGameRPC : StartGameC2SRPC
    {
        public override RPCAsyncResult StartGameC2SCall(StartGameC2S req)
        {
            if (req != null) req.IsAddBot = false;
            return base.StartGameC2SCall(req);
        }
    }
}
