using System.Diagnostics.CodeAnalysis;
using pl3xtweaks.client;
using pl3xtweaks.common;
using pl3xtweaks.server;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace pl3xtweaks;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public sealed class TweaksMod : ModSystem {
    private static TweaksMod Instance { get; set; } = null!;

    public static ICoreAPI? Api => Instance._api;
    public static string Id => Instance.Mod.Info.ModID;

    private ICoreAPI? _api;
    private AbstractTweaks? _tweaks;

    public TweaksMod() {
        Instance = this;
    }

    public override void StartClientSide(ICoreClientAPI api) {
        _api = api;
        _tweaks = new ClientTweaks(this, api);
    }

    public override void StartServerSide(ICoreServerAPI api) {
        _api = api;
        _tweaks = new ServerTweaks(this, api);
    }

    public override void Dispose() {
        _tweaks?.Dispose();
    }
}
