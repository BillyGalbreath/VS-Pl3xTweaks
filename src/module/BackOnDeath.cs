using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Vintagestory.API.Util;

namespace Pl3xTweaks.module;

public class BackOnDeath : Module {
    private readonly string _dataKey;

    private readonly ICoreServerAPI _api;

    public BackOnDeath(ICoreServerAPI api) {
        _api = api;
        _dataKey = "pl3xtweaks:deathpos";

        api.ChatCommands.Create("back")
            .WithDescription("Teleports you to your last death location")
            .WithAlias("return", "previous", "prev")
            .RequiresPlayer()
            .RequiresPrivilege(Privilege.chat)
            .HandleWith(Execute);

        api.Event.PlayerDeath += OnDeath;
        api.Event.PlayerRespawn += OnRespawn;
    }

    private void OnDeath(IServerPlayer player, DamageSource source) {
        player.SetModdata(_dataKey, SerializerUtil.Serialize(player.Entity.Pos));
    }

    private void OnRespawn(IServerPlayer player) {
        if (player.GetModdata(_dataKey) == null) {
            return;
        }
        player.SendMessage(GlobalConstants.GeneralChatGroup, "You can use /back to return to your death location.", EnumChatType.Notification);
    }

    private TextCommandResult Execute(TextCommandCallingArgs args) {
        if (args.Caller.Player is not IServerPlayer player) {
            return TextCommandResult.Error("Player command.");
        }

        EntityPos? pos = GetDeathPos(player);
        if (pos == null) {
            return TextCommandResult.Error("Unable to load back location.");
        }

        player.Entity.TeleportTo(pos);
        player.RemoveModdata(_dataKey);

        return TextCommandResult.Success("Returned to your death location.");
    }

    private EntityPos? GetDeathPos(IServerPlayer player) {
        byte[] data = player.GetModdata(_dataKey);
        return data == null ? null : SerializerUtil.Deserialize<EntityPos>(data);
    }

    public override void Dispose() {
        _api.Event.PlayerDeath -= OnDeath;
    }
}
