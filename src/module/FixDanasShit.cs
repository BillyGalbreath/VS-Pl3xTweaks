using System.Diagnostics.CodeAnalysis;
using pl3xtweaks;
using Vintagestory.API.Config;

namespace Pl3xTweaks.module;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class FixDanasShit : Module {
    public FixDanasShit(TweaksMod mod) {
        mod.Patch("ExtraInfo.Constants+Text", "CarburizationComplete", PrefixCarburizationText);
    }

    private static bool PrefixCarburizationText(int percent, ref string __result) {
        __result = Lang.Get("Carburization: {0}% complete", percent);
        return false;
    }
}
