using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace pl3xtweaks.module;

public class BoatSpeed : Module {
    private static readonly AssetLocation _sailed = new("game", "boat-sailed-*");

    public BoatSpeed(Pl3xTweaks mod) : base(mod) { }

    public override void AssetsFinalize(ICoreAPI api) {
        _mod.Patch(AccessTools.PropertyGetter(typeof(EntityBoat), "SpeedMultiplier"), postfix: Postfix);
    }

    private static void Postfix(EntityBoat __instance, ref float __result) {
        __result = WildcardUtil.Match(_sailed, __instance.Code) ? 4.0f : 2.0f;
    }
}
