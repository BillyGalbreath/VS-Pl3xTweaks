using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Pl3xTweaks.Extensions;
using Pl3xTweaks.Patches;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.Client;
using Vintagestory.Client.NoObf;
using Vintagestory.Common;
using Vintagestory.GameContent;

namespace Pl3xTweaks;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public sealed partial class TweaksMod : ModSystem {
    [GeneratedRegex(@"(\[item\])", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex ItemLinkGeneratedRegex();

    private ICoreAPI? _api;
    private HarmonyPatches? _harmony;
    private long _tickListenerId;

    public override void StartPre(ICoreAPI api) {
        _api = api;
    }

    public override void Start(ICoreAPI api) { }

    public override void AssetsFinalize(ICoreAPI api) {
        foreach (Block block in api.World.Blocks) {
            string? code = block.Code?.ToString();
            if (code == null) {
                continue;
            }

            if (block is BlockPie) {
                block.SetAttributeToken("shelvable", true);
                block.SetAttributeToken("onDisplayTransform", new ModelTransform {
                    Origin = new Vec3f(0.5F, 0F, 0.5F),
                    Scale = 0.65F
                });
            }

            if (code.Contains("claypot")) {
                block.SetAttributeToken("shelvable", true);
                block.SetAttributeToken("onDisplayTransform", new ModelTransform {
                    Origin = new Vec3f(0.5F, 0F, 0.5F),
                    Scale = 0.8F
                });
            }

            if (code.Contains("trapdoor")) {
                block.Climbable = true;
            }
        }

        foreach (Item item in api.World.Items) {
            string? code = item.Code?.ToString();
            if (code == null) {
                continue;
            }

            if (item is ItemFirestarter) {
                item.SetAttributeToken("rackable", true);
                item.SetAttributeToken("toolrackTransform", new ModelTransform {
                    Translation = new Vec3f(0.25F, 0.55F, 0.0275F),
                    Rotation = new Vec3f(180F, -135F, 0F),
                    Origin = new Vec3f(0.5F, 0F, 0.5F),
                    Scale = 0.7f
                });
            }

            if (code.Equals("game:gear-temporal", StringComparison.Ordinal)) {
                item.MaxStackSize = 16;
            }
        }
    }

    public override void StartClientSide(ICoreClientAPI api) {
        //api.Event.IsPlayerReady += OnReady;

        api.ChatCommands.Create("wireframe")
            .WithDescription("Shows the chunk wireframe")
            .WithRootAlias("showchunks")
            .HandleWith(_ => TextCommandResult.Success(Lang.Get("Chunk wireframe now {0}",
                api.ToggleWireframe() ? Lang.Get("on") : Lang.Get("off"))));
    }

    public override void StartServerSide(ICoreServerAPI api) {
        _tickListenerId = api.Event.RegisterGameTickListener(RemoveOffhandHunger, 500);

        api.Event.PlayerChat += OnPlayerChat;

        api.Event.RegisterCallback(_ => {
            ((ChatCommandApi)api.ChatCommands).GetField<Dictionary<string, IChatCommand>>("ichatCommands")!.Remove("nexttempstorm");
            api.ChatCommands.Create("nexttempstorm")
                .WithDescription("")
                .RequiresPrivilege(Privilege.chat)
                .HandleWith(_ => {
                    string message;
                    double days;

                    TemporalStormRunTimeData data = api.ModLoader.GetModSystem<SystemTemporalStability>().StormData;
                    if (data.nowStormActive) {
                        message = $"{data.nextStormStrength} temporal storm is still active for ";
                        days = data.stormActiveTotalDays - api.World.Calendar.TotalDays;
                    } else {
                        message = "Next temporal storm is in ";
                        days = data.nextStormTotalDays - api.World.Calendar.TotalDays;
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
                });
        }, 1);

        _harmony = new HarmonyPatches(this);
    }

    private void RemoveOffhandHunger(float obj) {
        foreach (IPlayer player in _api!.World.AllOnlinePlayers) {
            player.Entity?.Stats.Remove("hungerrate", "offhanditem");
        }
    }

    private bool OnReady(ref EnumHandling handling) {
        _api?.Event.RegisterCallback(_ => {
            ClientMain client = (ClientMain)_api.World;

            if (client.GetField<ServerConnectData>("ConnectData") is { Port: 42420 } data) {
                if (IsOurHost(data.Host) || IsOurHost(data.HostRaw)) {
                    return;
                }
            }

            client.EnqueueMainThreadTask(() => {
                const string reason = "\n\nThe Pl3xTweaks mod is for the Pl3x server only.\n\n<strong>Please uninstall Pl3xTweaks.</strong>";
                client.SetField("exitReason", reason);
                client.SetField("disconnectReason", reason);
                client.Invoke("DestroyGameSession", new object?[] { true });
            }, "disconnect");
        }, 1000);

        handling = EnumHandling.PassThrough;
        return true;
    }

    private bool IsOurHost(string hostName) {
        try {
            IPAddress ipv4 = IPAddress.Parse("45.59.171.117");
            IPAddress ipv6 = IPAddress.Parse("fe80::9e6b:ff:fe16:8783");

            if (IPAddress.TryParse(hostName, out IPAddress? ip)) {
                return ipv4.Equals(ip) || ipv6.Equals(ip);
            }

            IPAddress[] list = Dns.GetHostAddresses(hostName);
            return list is { Length: > 0 } && (
                ipv4.Equals(list.FirstOrDefault(ipAddress => ipAddress?.AddressFamily == AddressFamily.InterNetwork, null)) ||
                ipv6.Equals(list.FirstOrDefault(ipAddress => ipAddress?.AddressFamily == AddressFamily.InterNetworkV6, null))
            );
        } catch (Exception e) {
            Mod.Logger.Error(e.ToString());
            return false;
        }
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
            itemStack?.GetName() ?? Lang.Get("game:nothing"))}]";

        foreach (Match match in matches) {
            message = message.Replace(match.Value, replacement);
        }
    }

    public override void Dispose() {
        switch (_api) {
            case ICoreClientAPI capi:
                capi.Event.IsPlayerReady -= OnReady;
                break;
            case ICoreServerAPI sapi:
                sapi.Event.PlayerChat -= OnPlayerChat;
                break;
        }

        _api?.Event.UnregisterGameTickListener(_tickListenerId);

        _harmony?.Dispose();
    }
}
