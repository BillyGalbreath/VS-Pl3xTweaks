using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using pl3xtweaks;
using Vintagestory.API.Config;
using Vintagestory.Server;

namespace Pl3xTweaks.module;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class DeathMessageFix : Module {
    private static bool _bypassLanguage;

    public DeathMessageFix(TweaksMod mod) {
        mod.Patch<ServerMain>("HandleLeave", prefix: Pre, postfix: Post);
        mod.Patch<ServerSystemEntitySimulation>("GetDeathMessage", prefix: Pre, postfix: Post);
        mod.Patch(typeof(Lang).GetMethod("GetL", BindingFlags.Static | BindingFlags.Public), Fix);
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
