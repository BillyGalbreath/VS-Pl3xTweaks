using HarmonyLib;
using Vintagestory.GameContent;

namespace pl3xtweaks.common;

public abstract class AbstractTweaks {
    private readonly TweaksMod _mod;

    protected readonly Harmony Harmony;

    protected AbstractTweaks(TweaksMod mod) {
        _mod = mod;
        Harmony = new Harmony(_mod.Mod.Info.ModID);

        // todo = move this to json patch??
        ItemChisel.carvingTime = true;
    }

    public virtual void Dispose() {
        Harmony.UnpatchAll(_mod.Mod.Info.ModID);
    }
}
