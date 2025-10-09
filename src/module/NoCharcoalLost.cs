using HarmonyLib;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace pl3xtweaks.module;

public class NoCharcoalLost(Pl3xTweaks __mod) : Module(__mod) {
    public override void StartServerSide(ICoreServerAPI api) {
        _mod.Patch<BlockEntityCharcoalPit>("ConvertPit", postfix: Postfix, transpiler: Transpiler);
    }

    private static void Postfix(BlockEntityCharcoalPit __instance) {
        if (__instance.Api.World.BlockAccessor.GetBlock(__instance.Pos).Code.PathStartsWith("charcoalpile")) {
            __instance.Api.World.BlockAccessor.SetBlock(0, __instance.Pos);
        }
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> codes = new(instructions);

        for (int i = 0; i < codes.Count; i++) {
            if (!codes[i].operand?.ToString()?.Equals("0.125") ?? true) {
                continue;
            }

            codes.RemoveRange(i - 1, 13);
            break;
        }

        return codes.AsEnumerable();
    }
}
