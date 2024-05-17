using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using Vintagestory.Common;

namespace Pl3xTweaks.Patches.Common;

public class GameCalendarPatches {
    internal GameCalendarPatches(Harmony harmony) {
        _ = new SleepNoSkipNightPatch(harmony);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
    private class SleepNoSkipNightPatch : AbstractPatch {
        public SleepNoSkipNightPatch(Harmony harmony) : base(harmony) {
            Patch<GameCalendar>("RemoveTimeSpeedModifier", PrefixRemove);
            Patch<GameCalendar>("SetTimeSpeedModifier", PrefixSet);
        }

        public static bool PrefixRemove(string name) {
            return !"sleeping".Equals(name);
        }

        public static bool PrefixSet(string name) {
            return !"sleeping".Equals(name);
        }
    }
}
