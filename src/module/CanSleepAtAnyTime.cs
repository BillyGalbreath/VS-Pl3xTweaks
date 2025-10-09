using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace pl3xtweaks.module;

public class CanSleepAtAnyTime(Pl3xTweaks __mod) : Module(__mod) {
    public override void AssetsFinalize(ICoreAPI api) {
        _mod.Patch<BlockBed>("OnBlockInteractStart", Prefix);
    }

    private static void Prefix(IPlayer byPlayer) {
        if (byPlayer.Entity.GetBehavior("tiredness") is EntityBehaviorTiredness behavior) {
            behavior.Tiredness = float.MaxValue;
        }
    }
}
