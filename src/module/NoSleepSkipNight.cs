using Vintagestory.API.Common;
using Vintagestory.Common;

namespace pl3xtweaks.module;

public class NoSleepSkipNight(Pl3xTweaks __mod) : Module(__mod) {
    public override void AssetsFinalize(ICoreAPI api) {
        _mod.Patch<GameCalendar>("RemoveTimeSpeedModifier", Prefix);
        _mod.Patch<GameCalendar>("SetTimeSpeedModifier", Prefix);
    }

    private static bool Prefix(string name) {
        return !"sleeping".Equals(name);
    }
}
