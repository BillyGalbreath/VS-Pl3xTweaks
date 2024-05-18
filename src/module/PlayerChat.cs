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

        string replacement = $"[{(pageCode is { Length: > 0 } ?
            $"<a href=\"handbook://{pageCode}\">{itemStack!.GetName()}</a>" :
            itemStack?.GetName() ?? Lang.Get("nothing"))}]";

        foreach (Match match in matches) {
            message = message.Replace(match.Value, replacement);
        }
    }

    public override void Dispose() {
        _api.Event.PlayerChat -= OnPlayerChat;
    }
}
