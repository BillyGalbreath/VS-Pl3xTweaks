using pl3xtweaks.util;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace pl3xtweaks.module;

public class Trashcan : Module {
    public Trashcan(Pl3xTweaks mod) : base(mod) { }

    public override void Start(ICoreAPI api) {
        api.RegisterBlockClass("trashcan", typeof(TrashcanBlock));
        api.RegisterBlockEntityClass("betrashcan", typeof(BETrashcan));
    }

    private class TrashcanBlock : Block {
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

    private class BETrashcan : BlockEntityContainer {
        private readonly TrashcanInventory _inventory;

        private GuiTrashcan? _clientDialog;
        private bool _trashMoving;

        public override InventoryBase Inventory => _inventory;

        public override string InventoryClassName => "trashcan";

        public BETrashcan() {
            _inventory = new TrashcanInventory(null, null);
            _inventory.SlotModified += OnSlotModified;
        }

        private void MoveTrashDown(bool putIn, int startFrom = 3) {
            _trashMoving = true;
            if (putIn) {
                if (_inventory[1]!.Itemstack == null) {
                    _inventory[1]!.Itemstack = _inventory[0]!.Itemstack;
                    _inventory[1]!.MarkDirty();
                    _inventory[0]!.Itemstack = null;
                    _inventory[0]!.MarkDirty();
                    _trashMoving = false;
                    return;
                }
            }
            int curSlot = startFrom;
            while (curSlot > 0) {
                if (_inventory[curSlot]!.Itemstack != null) {
                    bool freeSlotFound = false;
                    for (int i = curSlot - 1; i > 0; i--)
                        if (_inventory[i]!.Itemstack == null) {
                            freeSlotFound = true;
                            break;
                        }
                    if (!freeSlotFound) {
                        for (int i = curSlot; i > 1; i--) {
                            _inventory[i]!.Itemstack = _inventory[i - 1]!.Itemstack;
                            _inventory[i]!.MarkDirty();
                            _inventory[i - 1]!.Itemstack = null;
                            _inventory[i - 1]!.MarkDirty();
                        }
                    }
                } else {
                    if (curSlot == 1) {
                        _inventory[1]!.Itemstack = _inventory[0]!.Itemstack;
                        _inventory[1]!.MarkDirty();
                        _inventory[0]!.Itemstack = null;
                        _inventory[0]!.MarkDirty();
                    } else {
                        for (int i = curSlot; i > 1; i--) {
                            _inventory[i]!.Itemstack = _inventory[i - 1]!.Itemstack;
                            _inventory[i]!.MarkDirty();
                            _inventory[i - 1]!.Itemstack = null;
                            _inventory[i - 1]!.MarkDirty();
                        }
                    }
                }
                curSlot--;
            }
            _trashMoving = false;
        }

        private void OnSlotModified(int slotid) {
            if (Api.World.Side == EnumAppSide.Client) return;
            if (_trashMoving) return;
            if (slotid == 0) {
                if (_inventory[0]!.Itemstack != null) {
                    MoveTrashDown(true);
                }
            } else {
                MoveTrashDown(false, slotid);
            }
        }

        public override void Initialize(ICoreAPI api) {
            base.Initialize(api);

            _inventory.LateInitialize("trashcan-1", api);
        }

        public void OnBlockInteract(IPlayer byPlayer) {
            if (Api.Side == EnumAppSide.Server) {
                byte[] data;

                using (MemoryStream ms = new()) {
                    BinaryWriter writer = new(ms);
                    TreeAttribute tree = new();
                    _inventory.ToTreeAttributes(tree);
                    tree.ToBytes(writer);
                    data = ms.ToArray();
                }

                ((ICoreServerAPI)Api).Network.SendBlockEntityPacket(
                    (IServerPlayer)byPlayer,
                    Pos.X, Pos.Y, Pos.Z,
                    (int)EnumBlockStovePacket.OpenGUI,
                    data
                );

                byPlayer.InventoryManager.OpenInventory(_inventory);
            }
        }

        public override void OnReceivedClientPacket(IPlayer fromPlayer, int packetid, byte[] data) {
            switch (packetid) {
                case <= 1000:
                    _inventory.InvNetworkUtil.HandleClientPacket(fromPlayer, packetid, data);
                    break;
                case (int)EnumBlockEntityPacketId.Close:
                    fromPlayer.InventoryManager?.CloseInventory(Inventory);
                    break;
            }
        }

        public override void OnReceivedServerPacket(int packetid, byte[] data) {
            base.OnReceivedServerPacket(packetid, data);
            switch (packetid) {
                case (int)EnumBlockStovePacket.OpenGUI: {
                    using MemoryStream ms = new(data);
                    BinaryReader reader = new(ms);
                    TreeAttribute tree = new();
                    tree.FromBytes(reader);
                    Inventory.FromTreeAttributes(tree);
                    Inventory.ResolveBlocksOrItems();

                    if (_clientDialog == null) {
                        _clientDialog = new GuiTrashcan(Lang.Get("block-trashcan"), Inventory, Pos, (ICoreClientAPI)Api);
                        _clientDialog.OnClosed += () => { _clientDialog = null; };
                    }

                    _clientDialog.TryOpen();
                    break;
                }
                case (int)EnumBlockEntityPacketId.Close:
                    ((IClientWorldAccessor)Api.World).Player.InventoryManager.CloseInventory(Inventory);
                    _clientDialog?.TryClose();
                    _clientDialog?.Dispose();
                    _clientDialog = null;
                    break;
            }
        }
    }

    private class GuiTrashcan : GuiDialogBlockEntity {
        public GuiTrashcan(string dialogTitle, InventoryBase inventory, BlockPos blockEntityPos, ICoreClientAPI capi) : base(dialogTitle, inventory, blockEntityPos, capi) {
            if (IsDuplicate) {
                return;
            }

            capi.World.Player.InventoryManager.OpenInventory(Inventory);

            SetupDialog();
        }

        private void SetupDialog() {
            ItemSlot? hoveredSlot = capi.World.Player.InventoryManager.CurrentHoveredSlot;
            if (hoveredSlot != null && hoveredSlot.Inventory == Inventory) {
                capi.Input.TriggerOnMouseLeaveSlot(hoveredSlot);
            } else {
                hoveredSlot = null;
            }

            ElementBounds mainBounds = ElementBounds.Fixed(0, 0, 100, 150);

            ElementBounds inputSlotBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 30, 30, 1, 1);
            ElementBounds storageSlotBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 30, 110, 1, 3);


            // 2. Around all that is 10 pixel padding
            ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;
            bgBounds.WithChildren(mainBounds);

            // 3. Finally Dialog
            ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.RightMiddle)
                .WithFixedAlignmentOffset(-GuiStyle.DialogToScreenPadding, 0);


            ClearComposers();
            SingleComposer = capi.Gui
                .CreateCompo("blockentitytrashcan" + BlockEntityPosition, dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar(DialogTitle, OnTitleBarClose)
                .BeginChildElements(bgBounds)
                .AddItemSlotGrid(Inventory, SendInvPacket, 1, new[] { 0 }, inputSlotBounds, "inputSlot")
                .AddItemSlotGrid(Inventory, SendInvPacket, 1, new[] { 1, 2, 3 }, storageSlotBounds, "storageSlots")
                .EndChildElements()
                .Compose();

            if (hoveredSlot != null) {
                SingleComposer.OnMouseMove(new MouseEvent(capi.Input.MouseX, capi.Input.MouseY));
            }
        }

        private void OnTitleBarClose() {
            TryClose();
        }

        private void SendInvPacket(object p) {
            capi.Network.SendBlockEntityPacket(BlockEntityPosition.X, BlockEntityPosition.Y, BlockEntityPosition.Z, p);
        }
    }

    private class TrashcanInventory : InventoryBase, ISlotProvider {
        public ItemSlot[] Slots { get; private set; }

        public override int Count => Slots.Length;

        public TrashcanInventory(string? inventoryId, ICoreAPI? api) : base(inventoryId, api) {
            // 0 - in/out slot (top)
            // 1-3 - out only slots
            Slots = GenEmptySlots(4);
            baseWeight = 4;
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

    private class TrashSlot : ItemSlot {
        public TrashSlot(InventoryBase inventory) : base(inventory) { }

        public override bool CanHold(ItemSlot itemstackFromSourceSlot) {
            return false;
        }

        public override bool CanTakeFrom(ItemSlot sourceSlot, EnumMergePriority priority = EnumMergePriority.AutoMerge) {
            return false;
        }
    }
}
