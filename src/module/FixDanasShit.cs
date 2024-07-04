using System.Diagnostics.CodeAnalysis;
using Vintagestory.API.Client;
using Vintagestory.API.Config;

namespace pl3xtweaks.module;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class FixDanasShit : Module {
    public FixDanasShit(Pl3xTweaks mod) : base(mod) { }

    public override void StartClientSide(ICoreClientAPI api) {
        _mod.Patch("ExtraInfo.Constants+Text", "CarburizationComplete", PrefixCarburizationText);
    }

    private static bool PrefixCarburizationText(int percent, ref string __result) {
        __result = Lang.Get("Carburization: {0}% complete", percent);
        return false;
    }
}
