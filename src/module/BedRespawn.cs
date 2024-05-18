using System.Text.RegularExpressions;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace Pl3xTweaks.module;

public partial class BedRespawn : Module {
    [GeneratedRegex("^bed-wood(aged)?-(head|feet)-(north|south|east|west)$")]
    private static partial Regex WoodBedsRegex();

    private readonly ICoreServerAPI _api;

    public BedRespawn(ICoreServerAPI api) {
        _api = api;
        api.Event.DidUseBlock += OnUseBlock;
        api.Event.DidBreakBlock += OnBreakBlock;
    }

    private void OnUseBlock(IServerPlayer player, BlockSelection block) {
        if (_api.World.BlockAccessor.GetBlock(block.Position) is not BlockBed bedBlock) {
            return;
        }
        if (WoodBedsRegex().Matches(bedBlock.Code.Path).Count == 0) {
            return;
        }
        if (player.GetSpawnPosition(false).AsBlockPos.Add(0, -1, 0) == block.Position) {
            return;
        }
        player.SetSpawnPosition(new PlayerSpawnPos(block.Position.X, block.Position.Y + 1, block.Position.Z));
        player.SendMessage(GlobalConstants.GeneralChatGroup, Lang.Get("pl3xtweaks:set-new-respawn-point"), EnumChatType.Notification);
    }

    private void OnBreakBlock(IServerPlayer player, int oldblockid, BlockSelection blocksel) {
        //
    }

    public override void Dispose() {
        _api.Event.DidUseBlock -= OnUseBlock;
        _api.Event.DidBreakBlock -= OnBreakBlock;
    }
}
