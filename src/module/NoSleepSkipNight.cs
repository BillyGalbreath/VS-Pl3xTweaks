using pl3xtweaks;
using Vintagestory.Common;

namespace Pl3xTweaks.module;

public class NoSleepSkipNight : Module {
    public NoSleepSkipNight(TweaksMod mod) {
        mod.Patch<GameCalendar>("RemoveTimeSpeedModifier", PrefixRemove);
        mod.Patch<GameCalendar>("SetTimeSpeedModifier", PrefixSet);
    }

    private static bool PrefixRemove(string name) {
        return !"sleeping".Equals(name);
    }

    private static bool PrefixSet(string name) {
        return !"sleeping".Equals(name);
    }
}
