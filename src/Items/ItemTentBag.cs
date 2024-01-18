using System;
using Pl3xTweaks.Configuration;
using Pl3xTweaks.Extensions;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace Pl3xTweaks.Items;

public class ItemTentBag : Item {
    private static readonly AssetLocation EmptyBag = new("pl3xtweaks:tentbag-empty");
    private static readonly AssetLocation PackedBag = new("pl3xtweaks:tentbag-packed");

    private static readonly AssetLocation[] BannedBlocks = {
        new("game:log-grown-*"),
        new("game:log-resin-*"),
        new("game:log-resinharvested-*"),
        new("game:statictranslocator-*"),
        new("game:teleporterbase"),
        new("game:crop-*"),
        new("game:herb-*"),
        new("game:mushroom-*"),
        new("game:smallberrybush-*"),
        new("game:bigberrybush-*"),
        new("game:water-*"),
        new("game:lava-*"),
        new("game:farmland-*"),
        new("game:rawclay-*"),
        new("game:peat-*"),
        new("game:rock-*"),
        new("game:ore-*"),
        new("game:crock-burned-*"),
        new("game:bowl-meal"),
        new("game:claypot-cooked"),
        new("game:anvil-*"),
        new("game:forge")
    };

    private static bool IsPlantOrRock(Block? block) => Config.ReplacePlantsAndRocks && block?.Replaceable is >= 5500 and <= 6500;
    private static bool IsAirOrNull(Block? block) => block is not { Replaceable: < 9505 };
    private static bool IsReplaceable(Block? block) => IsAirOrNull(block) || IsPlantOrRock(block);

    private static void SendClientError(EntityPlayer entity, string error) => TweaksMod.Instance.SendClientError(entity.Player, error);

    public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection? blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling) {
        if (blockSel == null || byEntity is not EntityPlayer entity) {
            return;
        }

        handling = EnumHandHandling.PreventDefaultAction;

        // do actual work on server only
        if (api.Side != EnumAppSide.Server) {
            return;
        }

        string contents = slot.Itemstack.Attributes.GetString("tent-contents");
        if (contents == null) {
            PackTent(entity, blockSel, slot);
        } else {
            UnpackTent(entity, blockSel, slot, contents);
        }
    }

    private static void PackTent(EntityPlayer entity, BlockSelection blockSel, ItemSlot slot) {
        BlockPos start = blockSel.Position.AddCopy(-Config.Radius, 1, -Config.Radius);
        BlockPos end = blockSel.Position.AddCopy(Config.Radius, Math.Max(Config.Height, 3), Config.Radius);

        if (!CanPack(entity, start, end)) {
            return;
        }

        // create schematic of area
        BlockSchematic bs = new();
        bs.AddAreaWithoutEntities(entity.World, start, end);
        bs.Pack(entity.World, start);

        // store data in tentbag and spawn on clicked block
        ItemStack packed = new(entity.World.GetItem(PackedBag), slot.StackSize);
        packed.Attributes.SetString("tent-contents", bs.ToJson());
        entity.World.SpawnItemEntity(packed, blockSel.Position.ToVec3d().Add(0, 1, 0));

        // clear area in world
        entity.World.BulkBlockAccessor.WalkBlocks(start, end, (block, posX, posY, posZ) => {
            if (block.BlockId != 0) {
                entity.World.BulkBlockAccessor.SetBlock(0, new BlockPos(posX, posY, posZ));
            }
        });
        entity.World.BulkBlockAccessor.Commit();

        // remove empty tentbag from inventory
        slot.TakeOutWhole();
        slot.MarkDirty();

        // consume player saturation
        entity.ReduceOnlySaturation(Config.BuildEffort);
    }

    private static void UnpackTent(EntityPlayer entity, BlockSelection blockSel, ItemSlot slot, string contents) {
        int y = IsPlantOrRock(entity.World.BlockAccessor.GetBlock(blockSel.Position)) ? 1 : 0;

        BlockPos start = blockSel.Position.AddCopy(-Config.Radius, 0 - y, -Config.Radius);
        BlockPos end = blockSel.Position.AddCopy(Config.Radius, Math.Max(Config.Height, 3), Config.Radius);

        if (!CanUnpack(entity, start, end)) {
            return;
        }

        // try load schematic data from json contents
        string? error = null;
        BlockSchematic bs = BlockSchematic.LoadFromString(contents, ref error);
        if (!string.IsNullOrEmpty(error)) {
            SendClientError(entity, Lang.Get("pl3xtweaks:tentbag-unpack-error"));
            return;
        }

        // paste the schematic into the world
        BlockPos adjustedStart = bs.AdjustStartPos(start.Add(Config.Radius, 1, Config.Radius), EnumOrigin.BottomCenter);
        bs.ReplaceMode = EnumReplaceMode.ReplaceAll;
        bs.Place(entity.World.BulkBlockAccessor, entity.World, adjustedStart);
        entity.World.BulkBlockAccessor.Commit();
        bs.PlaceEntitiesAndBlockEntities(entity.World.BlockAccessor, entity.World, adjustedStart, bs.BlockCodes, bs.ItemCodes);

        // drop empty tentbag on the ground and remove
        ItemStack empty = new(entity.World.GetItem(EmptyBag), slot.StackSize);
        entity.World.SpawnItemEntity(empty, blockSel.Position.ToVec3d().Add(0, 1 - y, 0));

        // remove empty tentbag from inventory
        slot.TakeOutWhole();
        slot.MarkDirty();

        // consume player saturation
        entity.ReduceOnlySaturation(Config.BuildEffort);
    }

    private static bool CanPack(EntityPlayer entity, BlockPos start, BlockPos end) {
        bool allowed = true;

        entity.World.BlockAccessor.SearchBlocks(start, end, (block, pos) => {
            if (!entity.World.Claims.TryAccess(entity.Player, pos, EnumBlockAccessFlags.BuildOrBreak)) {
                SendClientError(entity, Lang.Get("pl3xtweaks:tentbag-cannot-build"));
                return allowed = false;
            }

            // ReSharper disable once InvertIf
            if (IsBannedBlock(block.Code)) {
                SendClientError(entity, Lang.Get("pl3xtweaks:tentbag-illegal-item", block.Code));
                return allowed = false;
            }

            return true;
        });

        return allowed;
    }

    private static bool CanUnpack(EntityPlayer entity, BlockPos start, BlockPos end) {
        bool allowed = true;

        entity.World.BlockAccessor.SearchBlocks(start, end, (block, pos) => {
            if (!entity.World.Claims.TryAccess(entity.Player, pos, EnumBlockAccessFlags.BuildOrBreak)) {
                SendClientError(entity, Lang.Get("pl3xtweaks:tentbag-cannot-build"));
                return allowed = false;
            }

            if (pos.Y == start.Y) {
                if (!Config.RequireFloor || block.SideSolid[BlockFacing.indexUP]) {
                    return allowed = true;
                }

                SendClientError(entity, Lang.Get("pl3xtweaks:tentbag-solid-ground"));
                return allowed = false;
            }

            if (IsReplaceable(block)) {
                return allowed = true;
            }

            SendClientError(entity, Lang.Get("pl3xtweaks:tentbag-clear-area"));
            return allowed = false;
        });

        return allowed;
    }

    private static bool IsBannedBlock(AssetLocation? needle) {
        if (needle == null) {
            return false;
        }

        foreach (AssetLocation hay in BannedBlocks) {
            if (hay.Equals(needle)) {
                return true;
            }

            if (hay.IsWildCard && WildcardUtil.GetWildcardValue(hay, needle) != null) {
                return true;
            }
        }

        return false;
    }
}
