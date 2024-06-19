using System.IO;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace pl3xtweaks.block.trashcan;

public class BETrashcan : BlockEntityContainer {
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
                    _clientDialog = new GuiTrashcan(Lang.Get("pl3xtweaks:block-trashcan"), Inventory, Pos, (ICoreClientAPI)Api);
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
