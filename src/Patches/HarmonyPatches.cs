using HarmonyLib;

namespace Pl3xTweaks.Patches;

public abstract class HarmonyPatches {
    private readonly TweaksMod _mod;

    protected readonly Harmony Harmony;

    protected HarmonyPatches(TweaksMod mod) {
        _mod = mod;
        Harmony = new Harmony(_mod.Mod.Info.ModID);
    }

    public void Dispose() {
        Harmony.UnpatchAll(_mod.Mod.Info.ModID);
    }
}
