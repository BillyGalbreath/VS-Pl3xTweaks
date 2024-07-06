using System.Reflection;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.Server;

namespace pl3xtweaks.module;

public class FirstJoinMessage : Module {
    private static bool _firstJoin;

    public FirstJoinMessage(Pl3xTweaks mod) : base(mod) { }

    public override void StartServerSide(ICoreServerAPI api) {
        _mod.Patch<ServerMain>("HandleClientLoaded", Pre, Post);
        _mod.Patch(typeof(Lang).GetMethod("Get", BindingFlags.Static | BindingFlags.Public), Fix);
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
