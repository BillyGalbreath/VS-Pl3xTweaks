using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Vintagestory.Server;

namespace pl3xtweaks.module;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class DeathMessageFix : Module {
    private static bool _bypassLanguage;

    public DeathMessageFix(Pl3xTweaks mod) : base(mod) { }

    public override void StartServerSide(ICoreServerAPI api) {
        _mod.Patch<ServerMain>("HandleLeave", prefix: Pre, postfix: Post);
        _mod.Patch<ServerSystemEntitySimulation>("GetDeathMessage", prefix: Pre, postfix: Post);
        _mod.Patch(typeof(Lang).GetMethod("GetL", BindingFlags.Static | BindingFlags.Public), Fix);
    }

    private static void Pre() {
        _bypassLanguage = true;
    }

    private static void Post() {
        _bypassLanguage = false;
    }

    private static bool Fix(ref string __result, string key, params object[] args) {
        if (_bypassLanguage) {
            __result = Lang.Get(key, args);
        }
        return !_bypassLanguage;
    }
}
