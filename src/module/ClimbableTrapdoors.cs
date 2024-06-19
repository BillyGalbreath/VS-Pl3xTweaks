using Vintagestory.API.Common;

namespace pl3xtweaks.module;

public class ClimbableTrapdoors : Module {
    public ClimbableTrapdoors(ICoreAPI api) {
        foreach (Block block in api.World.Blocks) {
            string? code = block.Code?.ToString();
            if (code != null && code.Contains("trapdoor")) {
                block.Climbable = true;
            }
        }
    }
}
