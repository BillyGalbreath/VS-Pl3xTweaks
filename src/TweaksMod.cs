using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Pl3xTweaks.Extensions;
using Pl3xTweaks.Patches;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.Client;
using Vintagestory.Client.NoObf;
using Vintagestory.GameContent;

namespace Pl3xTweaks;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public sealed class TweaksMod : ModSystem {
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
        api.Event.IsPlayerReady += OnReady;
    }

    public override void StartServerSide(ICoreServerAPI api) {
        _tickListenerId = api.Event.RegisterGameTickListener(RemoveOffhandHunger, 500);

        _harmony = new HarmonyPatches(this);
    }

    private void RemoveOffhandHunger(float obj) {
        foreach (IPlayer player in _api!.World.AllOnlinePlayers) {
            player.Entity?.Stats.Remove("hungerrate", "offhanditem");
        }
    }

    private bool OnReady(ref EnumHandling handling) {
        _api?.Event.RegisterCallback(_ => {
            ClientMain main = (ClientMain)_api.World;
            object? val = typeof(ClientMain).GetField("Connectdata", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(main);

            if (val is ServerConnectData { Port: 42420 } data) {
                if (IsOurHost(data.Host) || IsOurHost(data.HostRaw)) {
                    return;
                }
            }

            main.EnqueueMainThreadTask((Action)(() => {
                const string reason = "The mod Pl3xTweaks is for the Pl3x server only. Please uninstall.";
                typeof(ClientMain).GetField("exitReason", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(main, reason);
                typeof(ClientMain).GetField("disconnectReason", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(main, reason);
                typeof(ClientMain).GetMethod("DestroyGameSession", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(main, new object?[] { true });
            }), "disconnect");
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

    public override void Dispose() {
        if (_api is ICoreClientAPI capi) {
            capi.Event.IsPlayerReady -= OnReady;
        }

        _api?.Event.UnregisterGameTickListener(_tickListenerId);

        _harmony?.Dispose();
    }
}
