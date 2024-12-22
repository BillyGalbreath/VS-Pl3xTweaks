using HarmonyLib;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace pl3xtweaks.module;

public class NoCokeLost : Module {
    public NoCokeLost(Pl3xTweaks mod) : base(mod) { }

    public override void StartServerSide(ICoreServerAPI api) {
        _mod.Patch<BlockEntityCoalPile>("onBurningTickServer", transpiler: Transpiler);
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> codes = new(instructions);

        for (int i = 0; i < codes.Count; i++) {
            if (!codes[i].operand?.ToString()?.Equals("coke") ?? true) {
                continue;
            }

            codes.RemoveRange(i + 8, 5);
            break;
        }

        return codes.AsEnumerable();
    }
}
