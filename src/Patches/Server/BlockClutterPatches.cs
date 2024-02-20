using System;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace Pl3xTweaks.Patches.Server;

public sealed class BlockClutterPatches {
    internal BlockClutterPatches(Harmony harmony) {
        _ = new DropClutterPatch(harmony);
    }

    private class DropClutterPatch : AbstractPatch {
        public DropClutterPatch(Harmony harmony) : base(harmony) {
            Patch<BlockClutter>("GetDrops", Prefix);
            Patch<BlockShapeFromAttributes>("GetDrops", Prefix);
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
        public static bool Prefix(BlockShapeFromAttributes __instance, ref ItemStack[] __result, IWorldAccessor world, BlockPos pos) {
            BEBehaviorShapeFromAttributes? beBehavior = __instance.GetBEBehavior<BEBehaviorShapeFromAttributes>(pos);
            if (beBehavior != null) {
                ItemStack? itemStack = __instance.OnPickBlock(world, pos);
                if (itemStack != null) {
                    beBehavior.Collected = true;
                    itemStack.Attributes.SetBool("collected", true);
                    __result = new[] { itemStack };
                    return false;
                }
            }

            world.PlaySoundAt(new AssetLocation("sounds/effect/toolbreak"), pos.X + 0.5, pos.Y + 0.5, pos.Z + 0.5, randomizePitch: false, range: 12f);
            __result = Array.Empty<ItemStack>();
            return false;
        }
    }
}
