using pl3xtweaks.server.command.argument;
using pl3xtweaks.server.configuration;
using Vintagestory.API.Server;

namespace Pl3xTweaks.server.module;

public class BroadcastTips {
    private readonly ICoreServerAPI _api;
    private readonly long _tickId;

    public BroadcastTips(ICoreServerAPI api) {
        _api = api;
        _tickId = _api.Event.RegisterGameTickListener(_ => TipsConfig.Tick(api), 1000);

        api.Event.RegisterCallback(_ => {
            api.ChatCommands.Create("tips")
                .WithDescription("Toggles server tips on and off")
                .RequiresPlayer()
                .RequiresPrivilege(Privilege.chat)
                .WithArgs(new OnOffArgParser("enabled"))
                .HandleWith(TipsConfig.Execute);
        }, 1);
    }

    public void Dispose() {
        _api.Event.UnregisterGameTickListener(_tickId);
    }
}
