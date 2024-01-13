using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.Client;
using Vintagestory.Client.NoObf;
using Vintagestory.GameContent;

namespace Pl3xTweaks;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public class Pl3xTweaksMod : ModSystem {
    //private ICoreClientAPI? _api;

    public override bool ShouldLoad(EnumAppSide side) {
        return true;
    }

    /*public override void StartClientSide(ICoreClientAPI api) {
        _api = api;

        _api.Event.PlayerJoin += OnJoin;
    }

    private void OnJoin(IClientPlayer player) {
        Mod.Logger.Event("####### OnJoin");

        ClientMain main = (ClientMain)_api!.World;
        object? val = typeof(ClientMain).GetField("Connectdata", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(main);
        
        if (val is ServerConnectData data) {
            if (data.Port == 42420) {
                return;
            }
        }
        
        typeof(ClientMain).GetField("exitReason", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(main, "The mod Pl3xTweaks is for the Pl3x server only. Please uninstall.");
        typeof(ClientMain).GetMethod("DestroyGameSession", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(main, new object?[] { true });
    }*/
}
