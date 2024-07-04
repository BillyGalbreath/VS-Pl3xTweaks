using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace pl3xtweaks.module;

public class ClimbableTrapdoors : Module {
    public ClimbableTrapdoors(Pl3xTweaks mod) : base(mod) { }

    public override void StartClientSide(ICoreClientAPI api) {
        foreach (Block block in _mod.Api.World.Blocks) {
            string? code = block.Code?.ToString();
            if (code != null && code.Contains("trapdoor")) {
                block.Climbable = true;
            }
        }
    }
}
