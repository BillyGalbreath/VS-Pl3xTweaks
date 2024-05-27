using System.Text.RegularExpressions;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace Pl3xTweaks.module;

public partial class BedRespawn : Module {
    [GeneratedRegex("^(.*)?bed-(.+)-(head|feet)-(north|south|east|west)$")]
    private static partial Regex WoodBedsRegex();

    private readonly ICoreServerAPI _api;

    public BedRespawn(ICoreServerAPI api) {
        _api = api;
        api.Event.DidUseBlock += OnUseBlock;
        api.Event.DidBreakBlock += OnBreakBlock;
    }

    private void OnUseBlock(IServerPlayer player, BlockSelection blocksel) {
        if (_api.World.BlockAccessor.GetBlock(blocksel.Position) is not BlockBed bedBlock) {
            return;
        }
        if (WoodBedsRegex().Matches(bedBlock.Code.Path).Count == 0) {
            return;
        }
        BlockPos pos = GetBedFeetPos(bedBlock, blocksel).AddCopy(0, 1, 0);
        if (player.GetSpawnPosition(false).AsBlockPos == pos) {
            return;
        }
        player.SetSpawnPosition(new PlayerSpawnPos(pos.X, pos.Y, pos.Z));
        player.SendMessage(GlobalConstants.GeneralChatGroup, Lang.Get("pl3xtweaks:set-new-respawn-point"), EnumChatType.Notification);
    }

    private void OnBreakBlock(IServerPlayer player, int oldblockid, BlockSelection blocksel) {
        if (_api.World.BlockAccessor.GetBlock(oldblockid) is not BlockBed bedBlock) {
            return;
        }
        if (WoodBedsRegex().Matches(bedBlock.Code.Path).Count == 0) {
            return;
        }
        BlockPos pos = GetBedFeetPos(bedBlock, blocksel).AddCopy(0, 1, 0);
        string self = Lang.Get("pl3xtweaks:cleared-respawn-point");
        string other = Lang.Get($"pl3xtweaks:cleared-respawn-point-by-other", player.PlayerName);
        foreach (IServerPlayer offline in (IServerPlayer[])_api.World.AllPlayers) {
            if (offline.GetSpawnPosition(false).AsBlockPos == pos) {
                offline.ClearSpawnPosition();
                offline.SendMessage(GlobalConstants.GeneralChatGroup, offline.PlayerUID == player.PlayerUID ? self : other, EnumChatType.Notification);
            }
        }
    }

    private static BlockPos GetBedFeetPos(BlockBed bedBlock, BlockSelection blocksel) {
        BlockPos? pos = blocksel.Position.Copy();
        if (bedBlock.LastCodePart(1) != "feet") {
            pos.Add(BlockFacing.FromCode(bedBlock.LastCodePart()));
        }
        return pos;
    }

    public override void Dispose() {
        _api.Event.DidUseBlock -= OnUseBlock;
        _api.Event.DidBreakBlock -= OnBreakBlock;
    }
}
