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
        mod.Patch<ServerSystemHeartbeat>("SendRegister", PreRegister, PostRegister);
    }

    private static bool PreHeartbeat(ServerSystemHeartbeat __instance, ServerMain ___server) {
        string serverName = CalculateServerName(___server);
        if (serverName.Equals(_lastServerName)) {
            return true;
        }
        ___server.EnqueueMainThreadTask(() => __instance.Invoke("SendRegister", null));
        return false;
    }

    private static void PreRegister(ServerMain ___server, out string __state) {
        __state = ___server.Config.ServerName;
        ___server.Config.ServerName = _lastServerName = CalculateServerName(___server);
    }

    private static void PostRegister(ServerMain ___server, string __state) {
        ___server.Config.ServerName = __state;
    }

    private static string CalculateServerName(ServerMain server) {
        GameCalendar calendar = (GameCalendar)server.Calendar;
        string month = Lang.Get($"month-{calendar.MonthName.ToString()}");
        string year = calendar.Year.ToString("0");
        string day = calendar.DayOfMonth.ToString("00");
        return string.Format(server.Config.ServerName, $"{month} {day} of Year {year}");
    }
}
