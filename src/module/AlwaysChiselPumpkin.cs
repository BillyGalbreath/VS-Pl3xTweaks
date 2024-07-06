using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace pl3xtweaks.module;

public class AlwaysChiselPumpkin : Module {
    private bool _original;

    public AlwaysChiselPumpkin(Pl3xTweaks mod) : base(mod) { }

    public override void StartServerSide(ICoreServerAPI api) {
        _original = ItemChisel.carvingTime;
        ItemChisel.carvingTime = true;
    }

    public override void Dispose() {
        ItemChisel.carvingTime = _original;
    }
}
