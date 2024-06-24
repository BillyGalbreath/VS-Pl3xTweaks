using System.Diagnostics.CodeAnalysis;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace pl3xtweaks.module;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class IngotMoldBoxes : Module {
    private static readonly Cuboidf[] OneMoldBoxes = {
        new(0.375f, 0f, 0.25f, 0.6875f, 0.1875f, 0.8125f)
    };

    private static readonly Cuboidf[] TwoMoldBoxes = {
        new(0.125f, 0f, 0.25f, 0.4375f, 0.1875f, 0.8125f),
        new(0.5625f, 0f, 0.25f, 0.875f, 0.1875f, 0.8125f)
    };

    public IngotMoldBoxes(Pl3xTweaks mod) : base(mod) {
        mod.Patch<BlockIngotMold>("GetSelectionBoxes", Prefix);
    }

    private static bool Prefix(ref Cuboidf[] __result, ICoreAPI ___api, BlockPos pos) {
        __result = ___api.World.BlockAccessor.GetBlockEntity(pos) is not BlockEntityIngotMold mold || mold.quantityMolds == 1 ? OneMoldBoxes : TwoMoldBoxes;
        return false;
    }
}
