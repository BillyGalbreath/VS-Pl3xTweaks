using Pl3xTweaks.Patches.Client;

namespace Pl3xTweaks.Patches;

public sealed class ClientHarmonyPatches : HarmonyPatches {
    public ClientHarmonyPatches(TweaksMod mod) : base(mod) {
        _ = new BlockIngotMoldPatches(Harmony);
    }
}
