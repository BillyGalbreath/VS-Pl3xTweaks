using Vintagestory.API.Client;
using Vintagestory.Common;

namespace pl3xtweaks.module;

public class NoSleepSkipNight : Module {
    public NoSleepSkipNight(Pl3xTweaks mod) : base(mod) { }

    public override void StartClientSide(ICoreClientAPI api) {
        _mod.Patch<GameCalendar>("RemoveTimeSpeedModifier", PrefixRemove);
        _mod.Patch<GameCalendar>("SetTimeSpeedModifier", PrefixSet);
    }

    private static bool PrefixRemove(string name) {
        return !"sleeping".Equals(name);
    }

    private static bool PrefixSet(string name) {
        return !"sleeping".Equals(name);
    }
}
