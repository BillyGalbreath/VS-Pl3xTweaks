using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace Pl3xTweaks.Patches;

public sealed class BlockClutterPatches {
    internal BlockClutterPatches(Harmony harmony) {
        _ = new DropClutterPatch(harmony);
    }

    private class DropClutterPatch : AbstractPatch {
        public DropClutterPatch(Harmony harmony) : base(harmony) {
            Patch<BlockClutter>("GetDrops", Prefix);
            Patch<BlockClutterBookshelf>("GetDrops", Prefix);
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
        public static bool Prefix(BlockShapeFromAttributes __instance, BlockPos pos) {
            __instance.GetBEBehavior<BEBehaviorShapeFromAttributes>(pos).Collected = true;
            return true;
        }
    }
}
