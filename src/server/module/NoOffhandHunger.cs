using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace Pl3xTweaks.server.module;

public class NoOffhandHunger {
    private readonly ICoreServerAPI _api;
    private readonly long _tickId;

    public NoOffhandHunger(ICoreServerAPI api) {
        _api = api;
        _tickId = _api.Event.RegisterGameTickListener(RemoveOffhandHunger, 500);
    }

    private void RemoveOffhandHunger(float obj) {
        foreach (IPlayer player in _api.World.AllOnlinePlayers) {
            player.Entity?.Stats.Remove("hungerrate", "offhanditem");
        }
    }

    public void Dispose() {
        _api.Event.UnregisterGameTickListener(_tickId);
    }
}
