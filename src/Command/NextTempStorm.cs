using Pl3xTweaks.Extensions;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace Pl3xTweaks.Command;

public abstract class NextTempStorm {
    public static TextCommandResult Execute(TextCommandCallingArgs _) {
        string message;
        double days;

        double totalDays = TweaksMod.Api!.World.Calendar.TotalDays;
        TemporalStormRunTimeData data = TweaksMod.Api.ModLoader.GetModSystem<SystemTemporalStability>().StormData;
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
