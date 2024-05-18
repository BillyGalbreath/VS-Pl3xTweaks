using System.Diagnostics.CodeAnalysis;
using pl3xtweaks;
using Vintagestory.GameContent;

namespace Pl3xTweaks.module;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class BetterFirepit : Module {
    public BetterFirepit(TweaksMod mod) {
        mod.Patch<BlockEntityFirepit>("smeltItems", Prefix, Postfix);
    }

    private static void Prefix(BlockEntityFirepit __instance, out float __state) {
        __state = __instance.InputStackTemp;
    }

    private static void Postfix(BlockEntityFirepit __instance, float __state) {
        __instance.InputStackTemp = __state;
    }
}
