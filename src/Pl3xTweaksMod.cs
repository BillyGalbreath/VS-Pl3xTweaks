using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace Pl3xTweaks;

public class Pl3xTweaksMod : ModSystem {
    public override bool ShouldLoad(EnumAppSide side) {
        return true;
    }

    public override void StartServerSide(ICoreServerAPI api) {
        //
    }
}
