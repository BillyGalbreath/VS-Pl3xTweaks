using Vintagestory.API.Common;

namespace pl3xtweaks.module;

public class ClimbableTrapdoors : Module {
    public ClimbableTrapdoors(Pl3xTweaks mod) : base(mod) { }

    public override void Start(ICoreAPI api) {
        foreach (Block block in api.World.Blocks) {
            if (block.Code?.ToString().Contains("trapdoor") ?? false) {
                block.Climbable = true;
            }
        }
    }
}
