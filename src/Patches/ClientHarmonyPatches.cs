using Pl3xTweaks.Patches.Client;
using Pl3xTweaks.Patches.Common;
using Pl3xTweaks.Patches.Server;

namespace Pl3xTweaks.Patches;

public sealed class ClientHarmonyPatches : HarmonyPatches {
    public ClientHarmonyPatches(TweaksMod mod) : base(mod) {
        _ = new BlockIngotMoldPatches(Harmony);
        _ = new GameCalendarPatches(Harmony);
        _ = new SystemTemporalStabilityPatches(Harmony);
    }
}
