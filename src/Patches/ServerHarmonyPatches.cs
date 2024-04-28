using Pl3xTweaks.Patches.Server;

namespace Pl3xTweaks.Patches;

public sealed class ServerHarmonyPatches : HarmonyPatches {
    public ServerHarmonyPatches(TweaksMod mod) : base(mod) {
        //_ = new BlockClutterPatches(Harmony); // 1.19.4 broke this
        _ = new BlockEntityCharcoalPitPatches(Harmony);
        _ = new BlockEntityCoalPilePatches(Harmony);
        _ = new BlockEntityPitKilnPatches(Harmony);
        _ = new EntityAgentPatches(Harmony);
        _ = new EntityBehaviorCollectEntitiesPatches(Harmony);
        _ = new FirepitPatches(Harmony);
    }
}
