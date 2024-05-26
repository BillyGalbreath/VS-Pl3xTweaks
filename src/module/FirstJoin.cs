using pl3xtweaks;
using Vintagestory.API.Config;
using Vintagestory.Server;

namespace Pl3xTweaks.module;

public class FirstJoin : Module {
    private string? _firstJoin;

    public FirstJoin(TweaksMod mod) {
        mod.Patch<ServerMain>("HandleClientLoaded", Prefix, Postfix);
        mod.Patch<ServerMain>("SendMessageToGeneral", PrefixMessage);
    }

    private void Prefix(ConnectedClient client) {
        if (client.IsNewEntityPlayer) {
            _firstJoin = Lang.Get("{0} joined for the first time! Say hi :)", client.PlayerName);
        }
    }

    private void Postfix() {
        _firstJoin = null;
    }

    private void PrefixMessage(ref string message) {
        if (_firstJoin != null) {
            message = _firstJoin;
        }
    }
}
