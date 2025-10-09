using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace pl3xtweaks.module;

public class PitKilnIgniteNeighbors(Pl3xTweaks __mod) : Module(__mod) {
    private static readonly Vec3i[] _diagonals = [new(1, 0, -1), new(1, 0, 1), new(-1, 0, 1), new(-1, 0, -1)];

    public override void StartServerSide(ICoreServerAPI api) {
        _mod.Patch<BlockEntityPitKiln>("TryIgnite", postfix: Postfix, types: [typeof(IPlayer)]);
    }

    private static void Postfix(BlockEntityPitKiln __instance, IPlayer? byPlayer) {
        foreach (Vec3i dir in _diagonals) {
            BlockPos pos = __instance.Pos.AddCopy(dir);
            __instance.Api.Event.RegisterCallback(_ => {
                BlockEntity blockEntity = __instance.Api.World.BlockAccessor.GetBlockEntity(pos);
                if (blockEntity is BlockEntityPitKiln { IsComplete: true, Lit: false } kiln) {
                    kiln.TryIgnite(byPlayer);
                }
            }, __instance.Api.World.Rand.Next(1000, 5000));
        }
    }
}
