using HarmonyLib;

namespace Pl3xTweaks.Patches;

public sealed class HarmonyPatches {
    private readonly TweaksMod _mod;

    private Harmony? _harmony;

    public HarmonyPatches(TweaksMod mod) {
        _mod = mod;
        _harmony = new Harmony(_mod.Mod.Info.ModID);

        _ = new BlockClutterPatches(_harmony);
        _ = new BlockEntityCharcoalPitPatches(_harmony);
        _ = new BlockEntityCoalPilePatches(_harmony);
        _ = new EntityAgentPatches(_harmony);
        _ = new FirepitPatches(_harmony);
    }

    public void Dispose() {
        _harmony?.UnpatchAll(_mod.Mod.Info.ModID);
        _harmony = null;
    }
}
