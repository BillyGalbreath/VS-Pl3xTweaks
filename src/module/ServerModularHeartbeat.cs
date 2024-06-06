using System.Diagnostics.CodeAnalysis;
using pl3xtweaks;
using Vintagestory.API.Config;
using Vintagestory.Common;
using Vintagestory.Server;

namespace Pl3xTweaks.module;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class ServerHeartbeat : Module {
    private static string? _lastServerName;

    public ServerHeartbeat(TweaksMod mod) {
        mod.Patch<ServerSystemHeartbeat>("SendHeartbeat", PreHeartbeat);
        mod.Patch<ServerSystemHeartbeat>("SendRegister", PreRegister);
    }

    private static void PreHeartbeat(ServerSystemHeartbeat __instance, ServerMain ___server) {
        if (!CalculateServerName(___server).Equals(_lastServerName)) {
            ___server.EnqueueMainThreadTask(() => __instance.Invoke("SendRegister", null));
        }
    }

    private static void PreRegister(ServerMain ___server) {
        ___server.Config.ServerName = _lastServerName = CalculateServerName(___server);
    }

    private static string CalculateServerName(ServerMain server) {
        GameCalendar calendar = (GameCalendar)server.Calendar;
        string month = Lang.Get($"month-{calendar.MonthName.ToString()}");
        string year = calendar.Year.ToString("0");
        string day = calendar.DayOfMonth.ToString("00");
        return $"[EN] Pl3x | {month} {day} of Year {year}";
    }
}
