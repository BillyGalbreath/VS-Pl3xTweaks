using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace pl3xtweaks.module;

public class NoOffhandHunger : Module {
    private ICoreServerAPI? _api;
    private long _tickId;

    public NoOffhandHunger(Pl3xTweaks mod) : base(mod) { }

    public override void StartServerSide(ICoreServerAPI api) {
        _api = api;
        _tickId = api.Event.RegisterGameTickListener(RemoveOffhandHunger, 500);
    }

    private void RemoveOffhandHunger(float obj) {
        foreach (IPlayer player in _api!.World.AllOnlinePlayers) {
            player.Entity?.Stats.Remove("hungerrate", "offhanditem");
        }
    }

    public override void Dispose() {
        _api?.Event.UnregisterGameTickListener(_tickId);
    }
}
