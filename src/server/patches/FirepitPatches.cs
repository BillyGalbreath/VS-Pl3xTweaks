using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using pl3xtweaks.common;
using Vintagestory.GameContent;

namespace pl3xtweaks.server.patches;

public sealed class FirepitPatches {
    internal FirepitPatches(Harmony harmony) {
        _ = new SmeltItemsAbstractPatch(harmony);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
    private class SmeltItemsAbstractPatch : AbstractPatch {
        public SmeltItemsAbstractPatch(Harmony harmony) : base(harmony) {
            Patch<BlockEntityFirepit>("smeltItems", Prefix, Postfix);
        }

        public static void Prefix(BlockEntityFirepit __instance, out float __state) {
            __state = __instance.InputStackTemp;
        }

        public static void Postfix(BlockEntityFirepit __instance, float __state) {
            __instance.InputStackTemp = __state;
        }
    }
}
