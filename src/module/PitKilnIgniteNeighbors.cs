using System.Diagnostics.CodeAnalysis;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace pl3xtweaks.module;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class PitKilnIgniteNeighbors : Module {
    private static readonly string[] Diagonals = { "nw", "ne", "se", "sw" };

    public PitKilnIgniteNeighbors(Pl3xTweaks mod) : base(mod) { }

    public override void StartServerSide(ICoreServerAPI api) {
        _mod.Patch<BlockEntityPitKiln>("TryIgnite", postfix: Postfix);
    }

    private static void Postfix(BlockEntityPitKiln __instance, IPlayer? byPlayer) {
        if (byPlayer == null) {
            return;
        }
        foreach (string dir in Diagonals) {
            BlockPos pos = __instance.Pos.AddCopy(Cardinal.FromInitial(dir).Normali);
            __instance.Api.Event.RegisterCallback(_ => {
                BlockEntity blockEntity = __instance.Api.World.BlockAccessor.GetBlockEntity(pos);
                if (blockEntity is BlockEntityPitKiln { IsComplete: true, Lit: false } kiln) {
                    kiln.TryIgnite(byPlayer);
                }
            }, __instance.Api.World.Rand.Next(1000, 5000));
        }
    }
}
