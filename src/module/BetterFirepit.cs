using System.Diagnostics.CodeAnalysis;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace pl3xtweaks.module;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class BetterFirepit : Module {
    public BetterFirepit(Pl3xTweaks mod) : base(mod) { }

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
