using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Pl3xTweaks.Command;
using Pl3xTweaks.Configuration;
using Pl3xTweaks.Extensions;
using Pl3xTweaks.Patches;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;
using Vintagestory.Common;
using Vintagestory.GameContent;

namespace Pl3xTweaks;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public sealed partial class TweaksMod : ModSystem {
    [GeneratedRegex(@"(\[item\])", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex ItemLinkGeneratedRegex();

    [GeneratedRegex("^bed-wood(aged)?-(head|feet)-(north|south|east|west)$")]
    private static partial Regex WoodBedsRegex();

    private static TweaksMod Instance { get; set; } = null!;

    public static ICoreAPI? Api => Instance._api;
    public static string Id => Instance.Mod.Info.ModID;
    public static ILogger Logger => Instance.Mod.Logger;

    private ICoreAPI? _api;
    private HarmonyPatches? _harmony;
    private long _offhandHungerTickId;
    private long _tipsTickId;

    public TweaksMod() {
        Instance = this;
    }

    public override void StartPre(ICoreAPI api) {
        _api = api;
        ItemChisel.carvingTime = true;
    }

    public override void Start(ICoreAPI api) { }

    public override void AssetsFinalize(ICoreAPI api) {
        foreach (Item item in api.World.Items) {
            string? code = item.Code?.ToString();
            if (code == null) {
                continue;
            }

            if (code.Equals("game:gear-temporal", StringComparison.Ordinal)) {
                item.MaxStackSize = 16;
            }
        }
    }

    public override void StartClientSide(ICoreClientAPI api) {
        api.ChatCommands.Create("wireframe")
            .WithDescription("Shows the chunk wireframe")
            .WithRootAlias("showchunks")
            .HandleWith(_ => TextCommandResult.Success(Lang.Get("Chunk wireframe now {0}",
                api.ToggleWireframe() ? Lang.Get("on") : Lang.Get("off"))));

        _harmony = new ClientHarmonyPatches(this);
    }

    public override void StartServerSide(ICoreServerAPI api) {
        _offhandHungerTickId = api.Event.RegisterGameTickListener(RemoveOffhandHunger, 500);
        _tipsTickId = api.Event.RegisterGameTickListener(_ => TipsConfig.Tick(api), 1000);

        api.Event.DidUseBlock += OnUseBlock;
        api.Event.PlayerChat += OnPlayerChat;

        api.Event.RegisterCallback(_ => {
            ((ChatCommandApi)api.ChatCommands).GetField<Dictionary<string, IChatCommand>>("ichatCommands")!.Remove("nexttempstorm");
            api.ChatCommands.Create("nexttempstorm")
                .WithDescription("Tells you the amount of days until the next storm")
                .RequiresPrivilege(Privilege.chat)
                .HandleWith(NextTempStorm.Execute);
            api.ChatCommands.Create("tips")
                .WithDescription("Toggles server tips on and off")
                .RequiresPlayer()
                .RequiresPrivilege(Privilege.chat)
                .WithArgs(new OnOffArgParser("enabled"))
                .HandleWith(TipsConfig.Execute);
        }, 1);

        _harmony = new ServerHarmonyPatches(this);
    }

    private void RemoveOffhandHunger(float obj) {
        try {
            foreach (IPlayer player in _api!.World.AllOnlinePlayers) {
                player.Entity?.Stats.Remove("hungerrate", "offhanditem");
            }
        } catch (Exception) {
            // ignore
        }
    }

    private static void OnUseBlock(IServerPlayer player, BlockSelection block) {
        if (player.Entity.World.BlockAccessor.GetBlock(block.Position) is not BlockBed bedBlock) {
            return;
        }
        if (WoodBedsRegex().Matches(bedBlock.Code.Path).Count == 0) {
            return;
        }
        if (player.GetSpawnPosition(false).AsBlockPos.Add(0, -1, 0) == block.Position) {
            return;
        }
        player.SetSpawnPosition(new PlayerSpawnPos(block.Position.X, block.Position.Y + 1, block.Position.Z));
        player.SendMessage(GlobalConstants.GeneralChatGroup, Lang.Get("pl3xtweaks:set-new-respawn-point"), EnumChatType.Notification);
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
        if (_api is ICoreServerAPI sapi) {
            sapi.Event.PlayerChat -= OnPlayerChat;
            sapi.Event.UnregisterGameTickListener(_offhandHungerTickId);
            sapi.Event.UnregisterGameTickListener(_tipsTickId);
        }

        Config.Dispose();

        _harmony?.Dispose();
    }
}
