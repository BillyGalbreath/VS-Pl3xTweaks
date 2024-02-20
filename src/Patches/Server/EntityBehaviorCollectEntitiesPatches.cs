using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HarmonyLib;
using Vintagestory.GameContent;

namespace Pl3xTweaks.Patches.Server;

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

            foreach (CodeInstruction code in codes.Where(code => code.operand is 1.5F)) {
                code.operand = 2.0F;
                break;
            }

            return codes.AsEnumerable();
        }
    }
}
