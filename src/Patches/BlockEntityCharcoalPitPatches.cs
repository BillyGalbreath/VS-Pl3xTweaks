using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HarmonyLib;
using Vintagestory.GameContent;

namespace Pl3xTweaks.Patches;

public sealed class BlockEntityCharcoalPitPatches {
    internal BlockEntityCharcoalPitPatches(Harmony harmony) {
        _ = new NoCharcoalLostPatch(harmony);
    }

    private class NoCharcoalLostPatch : AbstractPatch {
        public NoCharcoalLostPatch(Harmony harmony) : base(harmony) {
            Patch<BlockEntityCharcoalPit>("onBurningTickServer", transpiler: Transpiler);
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
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
}
