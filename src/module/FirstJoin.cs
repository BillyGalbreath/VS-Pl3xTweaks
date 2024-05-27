using pl3xtweaks;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Vintagestory.Server;

namespace Pl3xTweaks.module;

public class FirstJoin : Module {
    private static string? _firstJoin;

    public FirstJoin(TweaksMod mod) {
        mod.Patch<ServerMain>("HandleClientLoaded", Prefix, Postfix);
        mod.Patch<ServerMain>("SendMessage", PrefixMessage, types: new[] { typeof(IServerPlayer), typeof(int), typeof(string), typeof(EnumChatType), typeof(string) });
    }

    private static void Prefix(ConnectedClient client) {
        if (client.IsNewEntityPlayer) {
            _firstJoin = Lang.Get("{0} joined for the first time! Say hi :)", client.PlayerName);
        }
    }

    private static void Postfix() {
        _firstJoin = null;
    }

    private static void PrefixMessage(EnumChatType chatType, ref string message) {
        if (chatType == EnumChatType.JoinLeave && _firstJoin != null) {
            message = _firstJoin;
        }
    }
}
