using CesiumLoader.SDK.Manifests;

[assembly: ModManifest(
    "开局不补 Bot",
    "1.0.0",
    "TenkinMori",
    "创建房间点「开始」时强制 StartGameC2S.IsAddBot = false，不再弹「是否补充 Bot」，直接开局。",
    Permissions = ModPermission.ReadGameState | ModPermission.ModifyGameState | ModPermission.Network,
    SdkVersion = "3.1.1")]
