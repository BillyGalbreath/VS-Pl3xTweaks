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

            for (int i = 0; i < codes.Count; i++) {
                if (codes[i].operand?.ToString() != "1.5") {
                    continue;
                }

                codes[i].operand = 3.0F;
                codes[i + 1].operand = 2.0F;
            }

            return codes.AsEnumerable();
        }
    }
}
