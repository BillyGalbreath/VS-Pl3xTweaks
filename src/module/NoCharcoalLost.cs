using System.Reflection.Emit;
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
            if (codes[i].opcode == OpCodes.Brfalse_S &&
                codes[i + 1].opcode == OpCodes.Ldloc_2 &&
                codes[i + 2].opcode == OpCodes.Ldc_I4_0) {
                codes[i + 2] = new CodeInstruction(OpCodes.Ldc_I4, -0x400);
            }

            if ((codes[i].operand?.ToString() ?? "").Contains("charcoalPileId") &&
                codes[i + 1].opcode == OpCodes.Ldloc_2) {
                codes[i + 1] = new CodeInstruction(OpCodes.Ldc_I4_8);
            }
        }

        return codes.AsEnumerable();
    }
}
