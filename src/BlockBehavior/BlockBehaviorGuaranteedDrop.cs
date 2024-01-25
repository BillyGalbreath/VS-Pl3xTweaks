using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace Pl3xTweaks.BlockBehavior;

public class BlockBehaviorGuaranteedDrop : Vintagestory.API.Common.BlockBehavior {
    public BlockBehaviorGuaranteedDrop(Block block) : base(block) { }

    public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, ref float dropChanceMultiplier, ref EnumHandling handling) {
        handling = EnumHandling.PreventDefault;
        return new[] { block.OnPickBlock(world, pos) };
    }
}
