using Vintagestory.API.Common;

namespace pl3xtweaks.block.trashcan;

public class TrashcanBlock : Block {
    public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel) {
        BETrashcan? be = null;
        if (blockSel.Position != null) {
            be = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BETrashcan;
        }

        bool handled = base.OnBlockInteractStart(world, byPlayer, blockSel);
        if (!handled && !byPlayer.WorldData.EntityControls.Sneak && blockSel.Position != null) {
            be?.OnBlockInteract(byPlayer);
            return true;
        }

        return handled;
    }
}
