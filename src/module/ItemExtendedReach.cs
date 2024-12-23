using HarmonyLib;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace pl3xtweaks.module;

public class ItemExtendedReach : Module {
    public ItemExtendedReach(Pl3xTweaks mod) : base(mod) { }

    public override void StartServerSide(ICoreServerAPI api) {
        _mod.Patch<EntityBehaviorCollectEntities>("OnGameTick", transpiler: Transpiler);
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> codes = new(instructions);

        foreach (CodeInstruction code in codes.Where(code => code.operand?.ToString() == "1.5")) {
            code.operand = 2.5F;
            break;
        }

        return codes.AsEnumerable();
    }
}
