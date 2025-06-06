using pl3xtweaks.util;
using Vintagestory.API.Server;
using Vintagestory.Common;
using Vintagestory.Server;

namespace pl3xtweaks.module;

public class ServerNameAndDescription : Module {
    private static readonly PluralFormatProvider _pluralFormatProvider = new();

    private static string? _lastServerName;
    private static string? _lastServerDescription;

    private new static Pl3xTweaks _mod = null!;

    public ServerNameAndDescription(Pl3xTweaks mod) : base(mod) {
        _mod = mod;
    }

    public override void StartServerSide(ICoreServerAPI api) {
        _mod.Patch<ServerSystemHeartbeat>("SendHeartbeat", PreHeartbeat);
        _mod.Patch<ServerSystemHeartbeat>("SendRegister", PreRegister);
    }

    private static bool PreHeartbeat(ServerSystemHeartbeat __instance, ServerMain ___server) {
        if (ParseDate(___server, _mod.ServerData.ServerName).Equals(_lastServerName) &&
            ParseDate(___server, _mod.ServerData.ServerDescription).Equals(_lastServerDescription)) {
            return true;
        }
        ___server.EnqueueMainThreadTask(() => {
            __instance.Invoke("SendRegister");
            // delay the heartbeat to give the register time to run.
            // register sets player count to 0, heartbeat sets real one
            ___server.Api.Event.RegisterCallback(_ => __instance.SendHeartbeat(), 100);
        });
        return false;
    }

    private static void PreRegister(ServerMain ___server) {
        ___server.Config.ServerName = _lastServerName = ParseDate(___server, _mod.ServerData.ServerName);
        ___server.Config.ServerDescription = _lastServerDescription = ParseDate(___server, _mod.ServerData.ServerDescription);
    }

    private static string ParseDate(ServerMain server, string text) {
        GameCalendar calendar = (GameCalendar)server.Calendar;
        string month = Lang.Get($"game:month-{calendar.MonthName.ToString()}");
        string year = calendar.Year.ToString("0");
        string day = calendar.DayOfMonth.ToString("00");
        return string.Format(_pluralFormatProvider, text, month, day, year);
    }
}
