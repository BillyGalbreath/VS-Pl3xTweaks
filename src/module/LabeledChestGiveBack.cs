using HarmonyLib;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace pl3xtweaks.module;

public class LabeledChestGiveBack : Module {
    public LabeledChestGiveBack(Pl3xTweaks mod) : base(mod) { }

    public override void StartServerSide(ICoreServerAPI api) {
        _mod.Patch<BlockEntityLabeledChest>("OnReceivedClientPacket", transpiler: Transpiler);
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> codes = new(instructions);
        for (int i = 0; i < codes.Count; i++) {
            if (codes[i].operand?.ToString()?.Equals("0.85") ?? false) {
                codes[i] = new CodeInstruction(codes[i].opcode, 1.1);
                break;
            }
        }
        return codes.AsEnumerable();
    }
}
