using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HarmonyLib;
using Vintagestory.GameContent;

namespace Pl3xTweaks.Patches;

public class EntityBehaviorCollectEntitiesPatches {
    internal EntityBehaviorCollectEntitiesPatches(Harmony harmony) {
        _ = new OnGameTickPatch(harmony);
    }

    private class OnGameTickPatch : AbstractPatch {
        public OnGameTickPatch(Harmony harmony) : base(harmony) {
            Patch<EntityBehaviorCollectEntities>("OnGameTick", transpiler: Transpiler);
        }

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            List<CodeInstruction> codes = new(instructions);

            foreach (CodeInstruction code in codes.Where(code => code.operand?.ToString() == "1.5")) {
                code.operand = 1.75F;
            }

            return codes.AsEnumerable();
        }
    }
}
