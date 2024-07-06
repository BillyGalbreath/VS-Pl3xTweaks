using pl3xtweaks.util;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.Common;
using Vintagestory.GameContent;

namespace pl3xtweaks.module;

public class NextTempStorm : Module {
    private readonly PluralFormatProvider _pluralFormatProvider;

    public NextTempStorm(Pl3xTweaks mod) : base(mod) {
        _pluralFormatProvider = new PluralFormatProvider();
    }

    public override void StartServerSide(ICoreServerAPI api) {
        api.Event.RegisterCallback(_ => {
            ((ChatCommandApi)api.ChatCommands).GetField<Dictionary<string, IChatCommand>>("ichatCommands")!.Remove("nexttempstorm");
            api.ChatCommands.Create("nexttempstorm")
                .WithDescription("Tells you the amount of days until the next storm")
                .RequiresPrivilege(Privilege.chat)
                .HandleWith(_ => Execute(api));
        }, 1);
    }

    private TextCommandResult Execute(ICoreServerAPI api) {
        string message;
        double days;

        double totalDays = api.World.Calendar.TotalDays;
        TemporalStormRunTimeData data = api.ModLoader.GetModSystem<SystemTemporalStability>().StormData;
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

        return TextCommandResult.Success(string.Format(_pluralFormatProvider, message, (int)days, (int)hours, (int)minutes));
    }
}
