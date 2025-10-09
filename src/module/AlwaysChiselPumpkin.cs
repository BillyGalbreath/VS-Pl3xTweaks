using Vintagestory.API.Server;

namespace pl3xtweaks.module;

public class AlwaysChiselPumpkin(Pl3xTweaks __mod) : Module(__mod) {
    public override void StartServerSide(ICoreServerAPI api) {
        _mod.Patch("ItemChisel", "carvingTime", postfix: Postfix);
    }

    private static void Postfix(ref bool __result) {
        __result = true;
    }
}
