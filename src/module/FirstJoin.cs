using System.Reflection;
using pl3xtweaks;
using Vintagestory.API.Config;
using Vintagestory.API.Util;
using Vintagestory.Server;

namespace Pl3xTweaks.module;

public class FirstJoin : Module {
    private static bool _firstJoin;

    public FirstJoin(TweaksMod mod) {
        mod.Patch<ServerMain>("HandleClientLoaded", Pre, Post);
        mod.Patch(typeof(Lang).GetMethod("Get", BindingFlags.Static | BindingFlags.Public), Fix);
    }

    private static void Pre(ConnectedClient client) {
        _firstJoin = !SerializerUtil.Deserialize(client.WorldData.GetModdata("createCharacter"), false);
    }

    private static void Post() {
        _firstJoin = false;
    }

    private static void Fix(ref string key) {
        if (_firstJoin && key.Equals("{0} joined. Say hi :)")) {
            key = "{0} joined for the first time! Say hi :)";
        }
    }
}
