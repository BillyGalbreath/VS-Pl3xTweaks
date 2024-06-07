using System.Diagnostics.CodeAnalysis;
using pl3xtweaks;
using Pl3xTweaks.configuration;
using Vintagestory.API.Config;
using Vintagestory.Common;
using Vintagestory.Server;

namespace Pl3xTweaks.module;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class ServerHeartbeat : Module {
    private static string? _lastServerName;
    private static string? _lastServerDescription;

    public ServerHeartbeat(TweaksMod mod) {
        mod.Patch<ServerSystemHeartbeat>("SendHeartbeat", PreHeartbeat);
        mod.Patch<ServerSystemHeartbeat>("SendRegister", PreRegister);
    }

    private static bool PreHeartbeat(ServerSystemHeartbeat __instance, ServerMain ___server) {
        if (ServerName(___server).Equals(_lastServerName) && Config.ServerConfig.ServerDescription.Equals(_lastServerDescription)) {
            return true;
        }
        ___server.EnqueueMainThreadTask(() => {
            __instance.Invoke("SendRegister", null);
            // delay the heartbeat to give the register time to run.
            // register sets player count to 0, heartbeat sets real one
            ___server.Api.Event.RegisterCallback(_ => __instance.SendHeartbeat(), 100);
        });
        return false;
    }

    private static void PreRegister(ServerMain ___server) {
        ___server.Config.ServerName = _lastServerName = ServerName(___server);
        ___server.Config.ServerDescription = _lastServerDescription = Config.ServerConfig.ServerDescription;
    }

    private static string ServerName(ServerMain server) {
        GameCalendar calendar = (GameCalendar)server.Calendar;
        string month = Lang.Get($"month-{calendar.MonthName.ToString()}");
        string year = calendar.Year.ToString("0");
        string day = calendar.DayOfMonth.ToString("00");
        return Config.ServerConfig.ServerName.Format(month, day, year);
    }
}
