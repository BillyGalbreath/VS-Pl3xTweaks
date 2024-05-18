using System;
using System.Diagnostics.CodeAnalysis;
using pl3xtweaks;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace Pl3xTweaks.module;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class NoSurfaceInstability : Module {
    public NoSurfaceInstability(TweaksMod mod) {
        mod.Patch<SystemTemporalStability>("GetTemporalStability", Prefix, Postfix,
            types: new[] { typeof(double), typeof(double), typeof(double) });
    }

    private static bool Prefix(SystemTemporalStability __instance, double x, double y, double z, ref float __result, out float __state, ICoreAPI ___api) {
        __state = 1.0F;

        if (__instance.StormData.nowStormActive) {
            // we don't alter stability while a temporal storm is active
            return true;
        }

        float lowerLimit = ___api.World.SeaLevel - 15.0F;
        float upperLimit = lowerLimit + 10.0F;

        if (y <= lowerLimit) {
            // below our limit gets regular stability
            return true;
        }

        if (y >= upperLimit) {
            // above our limit has full stability (no instability)
            __state = 1.0F;
            __result = 1.5F;
            return false;
        }

        // between our limits we need to post process the result for blending
        __state -= 1.0F - Math.Clamp(InverseLerp(lowerLimit, upperLimit, (float)y), 0.0F, 1.0F);
        return true;
    }

    private static void Postfix(double y, ref float __result, float __state) {
        if (__state is < 1.0F and > 0.0F) {
            __result = Lerp(__result, 1.5F, __state);
        }
    }

    private static float Lerp(float a, float b, float t) {
        return a + t * (b - a);
    }

    private static float InverseLerp(float a, float b, float t) {
        return (t - a) / (b - a);
    }
}
