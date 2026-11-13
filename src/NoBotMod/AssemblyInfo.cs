using CesiumLoader.SDK.Manifests;

[assembly: ModManifest(
    "缺人时不补人机",
    "1.1.0",
    "TenkinMori",
    "在自定义房间缺人时不自动补充人机，直接开局。",
    Permissions = ModPermission.ReadGameState | ModPermission.ModifyGameState | ModPermission.Network,
    SdkVersion = "3.1.1")]
