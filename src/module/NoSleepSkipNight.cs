using Vintagestory.API.Common;
using Vintagestory.Common;

namespace pl3xtweaks.module;

public class NoSleepSkipNight : Module {
    public NoSleepSkipNight(Pl3xTweaks mod) : base(mod) { }

    public override void Start(ICoreAPI api) {
        _mod.Patch<GameCalendar>("RemoveTimeSpeedModifier", Prefix);
        _mod.Patch<GameCalendar>("SetTimeSpeedModifier", Prefix);
    }

    private static bool Prefix(string name) {
        return !"sleeping".Equals(name);
    }
}
