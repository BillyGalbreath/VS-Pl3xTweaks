using System.Text.RegularExpressions;
using pl3xtweaks.util;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace pl3xtweaks.module;

public partial class ItemInChat : Module {
    [GeneratedRegex(@"(\[item\])", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex ItemLinkGeneratedRegex();

    private ICoreServerAPI? _api;

    public ItemInChat(Pl3xTweaks mod) : base(mod) { }

    public override void StartServerSide(ICoreServerAPI api) {
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
            itemlink = itemStack?.GetName() ?? Lang.Get("game:nothing");
        }

        string replacement = $"[{itemlink}]";

        foreach (Match match in matches) {
            message = message.Replace(match.Value, replacement);
        }
    }

    public override void Dispose() {
        if (_api != null) {
            _api.Event.PlayerChat -= OnPlayerChat;
        }
    }
}
