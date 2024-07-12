using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace pl3xtweaks.module;

public class CanSleepAtAnyTime : Module {
    public CanSleepAtAnyTime(Pl3xTweaks mod) : base(mod) { }

    public override void AssetsFinalize(ICoreAPI api) {
        _mod.Patch<BlockBed>("OnBlockInteractStart", Prefix);
    }

    private static void Prefix(IPlayer byPlayer) {
        if (byPlayer.Entity.GetBehavior("tiredness") is EntityBehaviorTiredness behavior) {
            behavior.Tiredness = float.MaxValue;
        }
    }
}
