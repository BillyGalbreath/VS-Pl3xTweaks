using Pl3xTweaks.configuration;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace Pl3xTweaks.module;

public class BroadcastTips : Module {
    private readonly long _tickId;

    private readonly ICoreServerAPI _api;

    public BroadcastTips(ICoreServerAPI api) {
        _api = api;
        _tickId = _api.Event.RegisterGameTickListener(_ => TipsConfig.Tick(_api), 1000);

        _api.Event.RegisterCallback(_ => {
            _api.ChatCommands.Create("tips")
                .WithDescription("Toggles server tips on and off")
                .RequiresPlayer()
                .RequiresPrivilege(Privilege.chat)
                .WithArgs(new BoolArgParser("enabled", "true", true))
                .HandleWith(TipsConfig.Execute);
        }, 1);
    }

    public override void Dispose() {
        _api.Event.UnregisterGameTickListener(_tickId);
    }
}
