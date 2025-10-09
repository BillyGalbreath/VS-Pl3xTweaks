using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace pl3xtweaks.module;

public class BetterFirepit(Pl3xTweaks __mod) : Module(__mod) {
    public override void StartServerSide(ICoreServerAPI api) {
        _mod.Patch<BlockEntityFirepit>("smeltItems", Prefix, Postfix);
    }

    private static void Prefix(BlockEntityFirepit __instance, out float __state) {
        __state = __instance.InputStackTemp;
    }

    private static void Postfix(BlockEntityFirepit __instance, float __state) {
        __instance.InputStackTemp = __state;
    }
}
