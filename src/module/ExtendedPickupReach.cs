using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using pl3xtweaks;
using Vintagestory.GameContent;

namespace Pl3xTweaks.module;

public class ExtendedPickupReach : Module {
    public ExtendedPickupReach(TweaksMod mod) {
        mod.Patch<EntityBehaviorCollectEntities>("OnGameTick", transpiler: Transpiler);
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> codes = new(instructions);

        foreach (CodeInstruction code in codes.Where(code => code.operand is 1.5F)) {
            code.operand = 2.0F;
            break;
        }

        return codes.AsEnumerable();
    }
}
