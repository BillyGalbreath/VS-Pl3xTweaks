using System.Diagnostics.CodeAnalysis;
using pl3xtweaks;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;

namespace Pl3xTweaks.module;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class CooperativeCombat : Module {
    public CooperativeCombat(TweaksMod mod) {
        mod.Patch<EntityAgent>("ReceiveDamage", Prefix);
    }

    private static void Prefix(EntityAgent __instance, DamageSource damageSource, float damage) {
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
