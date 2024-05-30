using System;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace Pl3xTweaks.block.trashcan;

public class TrashcanInventory : InventoryBase, ISlotProvider {
    public ItemSlot[] Slots { get; private set; }

    public override int Count => Slots.Length;

    public TrashcanInventory(string? inventoryId, ICoreAPI? api) : base(inventoryId, api) {
        // 0 - in/out slot (top)
        // 1-3 - out only slots
        Slots = GenEmptySlots(4);
        baseWeight = 4;
    }

    public TrashcanInventory(string className, string instanceId, ICoreAPI? api) : base(className, instanceId, api) {
        Slots = GenEmptySlots(4);
    }

    public override ItemSlot? this[int slotId] {
        get {
            if (slotId < 0 || slotId >= Count) {
                return null;
            }
            return Slots[slotId];
        }
        set {
            if (slotId < 0 || slotId >= Count) {
                throw new ArgumentOutOfRangeException(nameof(slotId));
            }
            Slots[slotId] = value ?? throw new ArgumentNullException(nameof(value));
        }
    }

    protected override ItemSlot NewSlot(int slotId) {
        if (slotId != 0) {
            return new TrashSlot(this);
        }
        return new ItemSlotSurvival(this);
    }

    public override void FromTreeAttributes(ITreeAttribute tree) {
        Slots = SlotsFromTreeAttributes(tree);
    }

    public override void ToTreeAttributes(ITreeAttribute tree) {
        SlotsToTreeAttributes(Slots, tree);
    }
}
