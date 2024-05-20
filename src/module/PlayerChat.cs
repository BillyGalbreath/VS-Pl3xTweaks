using System.Text.RegularExpressions;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace Pl3xTweaks.module;

public partial class PlayerChat : Module {
    [GeneratedRegex(@"(\[item\])", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex ItemLinkGeneratedRegex();

    private readonly ICoreServerAPI _api;

    public PlayerChat(ICoreServerAPI api) {
        _api = api;
        api.Event.PlayerChat += OnPlayerChat;
    }

    private static void OnPlayerChat(IServerPlayer sender, int channel, ref string message, ref string data, BoolRef consumed) {
        MatchCollection matches = ItemLinkGeneratedRegex().Matches(message);
        if (matches.Count == 0) {
            return;
        }

        int slotNum = sender.InventoryManager.ActiveHotbarSlotNumber;
        ItemStack itemStack = sender.InventoryManager.GetHotbarItemstack(slotNum);
        string pageCode = itemStack == null ? "" : GuiHandbookItemStackPage.PageCodeForStack(itemStack);

        string itemlink;
        if (pageCode is { Length: > 0 }) {
            string name;
            if (pageCode.StartsWith("item-tentbag:tentbag-packed")) {
                pageCode = "item-tentbag:tentbag-packed";
                name = Lang.Get("tentbag:item-tentbag-packed");
            } else {
                name = itemStack!.GetName();
            }
            itemlink = $"<a href=\"handbook://{pageCode}\">{name}</a>";
        } else {
            itemlink = itemStack?.GetName() ?? Lang.Get("nothing");
        }

        string replacement = $"[{itemlink}]";

        foreach (Match match in matches) {
            message = message.Replace(match.Value, replacement);
        }
    }

    public override void Dispose() {
        _api.Event.PlayerChat -= OnPlayerChat;
    }
}
