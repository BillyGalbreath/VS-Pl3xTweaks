using Vintagestory.API.Common;

namespace pl3xtweaks.block.trashcan;

public class TrashSlot : ItemSlot {
    public TrashSlot(InventoryBase inventory) : base(inventory) { }

    public override bool CanHold(ItemSlot itemstackFromSourceSlot) {
        return false;
    }

    public override bool CanTakeFrom(ItemSlot sourceSlot, EnumMergePriority priority = EnumMergePriority.AutoMerge) {
        return false;
    }
}
