using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace pl3xtweaks.module;

public class IngotMoldBoxes : Module {
    private static readonly Cuboidf[] _oneMoldBoxesNorth = {
        new(0.375f, 0f, 0.25f, 0.6875f, 0.1875f, 0.8125f) // 6,4  11,13
    };

    private static readonly Cuboidf[] _oneMoldBoxesEast = {
        new(0.1875f, 0f, 0.375f, 0.75f, 0.1875f, 0.6875f) // 3,6  12,11
    };

    private static readonly Cuboidf[] _oneMoldBoxesSouth = {
        new(0.3125f, 0f, 0.1875f, 0.625f, 0.1875f, 0.75f) // 5,3  10,12
    };

    private static readonly Cuboidf[] _oneMoldBoxesWest = {
        new(0.25f, 0f, 0.3125f, 0.8125f, 0.1875f, 0.625f) // 4,5  13,10  (0.0625)
    };

    private static readonly Cuboidf[] _twoMoldBoxesNorth = {
        new(0.125f, 0f, 0.25f, 0.4375f, 0.1875f, 0.8125f), // 2,4  7,13
        new(0.5625f, 0f, 0.25f, 0.875f, 0.1875f, 0.8125f) // 9,4  14,13
    };

    private static readonly Cuboidf[] _twoMoldBoxesEast = {
        new(0.1875f, 0f, 0.125f, 0.75f, 0.1875f, 0.4375f), // 3,2  12,7
        new(0.1875f, 0f, 0.5625f, 0.75f, 0.1875f, 0.875f) // 3,9  12,14
    };

    private static readonly Cuboidf[] _twoMoldBoxesSouth = {
        new(0.125f, 0f, 0.1875f, 0.4375f, 0.1875f, 0.75f), // 2,3  7,12
        new(0.5625f, 0f, 0.1875f, 0.875f, 0.1875f, 0.75f) // 9,3  14,12
    };

    private static readonly Cuboidf[] _twoMoldBoxesWest = {
        new(0.25f, 0f, 0.125f, 0.8125f, 0.1875f, 0.4375f), // 4,2  13,7
        new(0.25f, 0f, 0.5625f, 0.8125f, 0.1875f, 0.875f) // 4,9  13,14
    };

    public IngotMoldBoxes(Pl3xTweaks mod) : base(mod) { }

    public override void StartClientSide(ICoreClientAPI api) {
        _mod.Patch<BlockIngotMold>("GetSelectionBoxes", Prefix);
    }

    private static bool Prefix(ref Cuboidf[] __result, ICoreAPI ___api, BlockPos pos) {
        if (___api.World.BlockAccessor.GetBlockEntity(pos) is not BlockEntityIngotMold blockEntity) {
            return true;
        }

        switch (BlockFacing.HorizontalFromAngle(blockEntity.MeshAngle).Index) {
            // the models all seem to be rotated 90 degrees CCW from their facing index
            case BlockFacing.indexNORTH:
                __result = blockEntity.QuantityMolds == 1 ? _oneMoldBoxesWest : _twoMoldBoxesWest;
                return false;
            case BlockFacing.indexEAST:
                __result = blockEntity.QuantityMolds == 1 ? _oneMoldBoxesNorth : _twoMoldBoxesNorth;
                return false;
            case BlockFacing.indexSOUTH:
                __result = blockEntity.QuantityMolds == 1 ? _oneMoldBoxesEast : _twoMoldBoxesEast;
                return false;
            case BlockFacing.indexWEST:
                __result = blockEntity.QuantityMolds == 1 ? _oneMoldBoxesSouth : _twoMoldBoxesSouth;
                return false;
            default:
                return true;
        }
    }
}
