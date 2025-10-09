using Vintagestory.API.Common;

namespace pl3xtweaks.module;

public class ClimbableTrapdoors(Pl3xTweaks __mod) : Module(__mod) {
    public override void AssetsFinalize(ICoreAPI api) {
        foreach (Block block in api.World.Blocks) {
            string? code = block.Code?.ToString();
            if (code != null && code.Contains("trapdoor")) {
                block.Climbable = true;
            }
        }
    }
}
