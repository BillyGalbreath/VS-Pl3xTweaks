using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;

namespace Pl3xTweaks.Patches.Server;

public class EntityAgentPatches {
    internal EntityAgentPatches(Harmony harmony) {
        _ = new CooperativeCombatPatch(harmony);
    }

    private class CooperativeCombatPatch : AbstractPatch {
        public CooperativeCombatPatch(Harmony harmony) : base(harmony) {
            Patch<EntityAgent>("ReceiveDamage", Prefix);
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
        public static void Prefix(EntityAgent __instance, DamageSource damageSource, float damage) {
            if (__instance is EntityPlayer) {
                return;
            }

            Entity cause = damageSource.GetCauseEntity();
            if (cause is not EntityPlayer) {
                return;
            }

            if (__instance.IsActivityRunning("cc-" + cause.EntityId)) {
                return;
            }

            __instance.SetActivityRunning("invulnerable", -1);
            __instance.SetActivityRunning("cc-" + cause.EntityId, 500);
        }
    }
}
