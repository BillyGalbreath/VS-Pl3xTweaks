using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HarmonyLib;
using Vintagestory.GameContent;

namespace Pl3xTweaks.Patches.Server;

public sealed class BlockEntityCoalPilePatches {
    internal BlockEntityCoalPilePatches(Harmony harmony) {
        _ = new NoCoalLostPatch(harmony);
    }

    private class NoCoalLostPatch : AbstractPatch {
        public NoCoalLostPatch(Harmony harmony) : base(harmony) {
            Patch<BlockEntityCoalPile>("onBurningTickServer", transpiler: Transpiler);
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            List<CodeInstruction> codes = new(instructions);

            for (int i = 0; i < codes.Count; i++) {
                CodeInstruction code = codes[i];
                if (!code.operand?.ToString()?.Equals("coke") ?? true) {
                    continue;
                }

                codes.RemoveRange(i + 8, 5);
                break;
            }

            return codes.AsEnumerable();
        }
    }
}
