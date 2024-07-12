using System.Diagnostics.CodeAnalysis;
using pl3xtweaks.util;
using Vintagestory.API.Client;

namespace pl3xtweaks.module;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class FixDanasShit : Module {
    public FixDanasShit(Pl3xTweaks mod) : base(mod) { }

    public override void StartClientSide(ICoreClientAPI api) {
        _mod.Patch("ExtraInfo.Constants+Text", "CarburizationComplete", PrefixCarburizationText);
    }

    private static bool PrefixCarburizationText(int percent, ref string __result) {
        __result = Lang.Get("game:Carburization: {0}% complete", percent);
        return false;
    }
}
