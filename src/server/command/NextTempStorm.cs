using System.Collections.Generic;
using pl3xtweaks.common;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.Common;
using Vintagestory.GameContent;

namespace pl3xtweaks.server.command;

public class NextTempStorm {
    private readonly ICoreServerAPI _api;

    public NextTempStorm(ICoreServerAPI api) {
        _api = api;

        api.Event.RegisterCallback(_ => {
            ((ChatCommandApi)api.ChatCommands).GetField<Dictionary<string, IChatCommand>>("ichatCommands")!.Remove("nexttempstorm");
            api.ChatCommands.Create("nexttempstorm")
                .WithDescription("Tells you the amount of days until the next storm")
                .RequiresPrivilege(Privilege.chat)
                .HandleWith(Execute);
        }, 1);
    }

    private TextCommandResult Execute(TextCommandCallingArgs _) {
        string message;
        double days;

        double totalDays = _api.World.Calendar.TotalDays;
        TemporalStormRunTimeData data = _api.ModLoader.GetModSystem<SystemTemporalStability>().StormData;
        if (data.nowStormActive) {
            message = $"{data.nextStormStrength} temporal storm is still active for ";
            days = data.stormActiveTotalDays - totalDays;
        } else {
            message = "Next temporal storm is in ";
            days = data.nextStormTotalDays - totalDays;
        }

        double hours = days * 24 % 24;
        double minutes = hours * 60 % 60;

        if ((int)days > 0) {
            message += "{0:day;days}, {1:hour;hours}, and {2:minute;minutes}";
        } else if ((int)hours > 0) {
            message += "{1:hour;hours} and {2:minute;minutes}";
        } else {
            message += "{2:minute;minutes}";
        }

        return TextCommandResult.Success(message.Format((int)days, (int)hours, (int)minutes));
    }
}
