using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Newtonsoft.Json.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.Client;
using Vintagestory.Client.NoObf;
using Vintagestory.GameContent;

namespace Pl3xTweaks;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public class Pl3xTweaksMod : ModSystem {
    private ICoreClientAPI? _capi;
    private ICoreServerAPI? _sapi;

    public override bool ShouldLoad(EnumAppSide side) {
        return true;
    }

    public override void StartClientSide(ICoreClientAPI capi) {
        _capi = capi;
        _capi.Event.IsPlayerReady += OnReady;
    }

    public override void StartServerSide(ICoreServerAPI sapi) {
        _sapi = sapi;
        _sapi.Event.RegisterGameTickListener(RemoveOffhandHunger, 500);
    }

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

            if (code.Equals("game:gear-temporal")) {
                item.MaxStackSize = 16;
            }
        }
    }

    private void RemoveOffhandHunger(float obj) {
        foreach (IPlayer player in _sapi!.World.AllOnlinePlayers) {
            player.Entity?.Stats.Remove("hungerrate", "offhanditem");
        }
    }

    private bool OnReady(ref EnumHandling handling) {
        _capi!.Event.RegisterCallback(_ => {
            ClientMain main = (ClientMain)_capi.World;
            object? val = typeof(ClientMain).GetField("Connectdata", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(main);

            if (val is ServerConnectData { Port: 42420 } data) {
                if (IsPl3x(data.Host) || IsPl3x(data.HostRaw)) {
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

    private bool IsPl3x(string hostName) {
        try {
            IPAddress ipv4 = IPAddress.Parse("45.59.171.117");
            IPAddress ipv6 = IPAddress.Parse("fe80::9e6b:ff:fe16:8783");

            if (IPAddress.TryParse(hostName, out IPAddress? ip)) {
                return ipv4.Equals(ip) || ipv6.Equals(ip);
            }

            IPAddress[] list = Dns.GetHostAddresses(hostName);
            if (list.Length == 0) {
                return false;
            }

            return ipv4.Equals(GetIP(list, AddressFamily.InterNetwork)) ||
                   ipv6.Equals(GetIP(list, AddressFamily.InterNetworkV6));
        } catch (Exception e) {
            Mod.Logger.Error(e.ToString());
            return false;
        }
    }

    private static IPAddress? GetIP(IEnumerable<IPAddress> list, AddressFamily family) {
        return list.FirstOrDefault(ipAddress => ipAddress?.AddressFamily == family, null);
    }
}

public static class Extensions {
    public static void SetAttributeToken(this CollectibleObject obj, string attr, object? val) {
        (obj.Attributes ??= new JsonObject(new JObject()))
            .Token[attr] = val == null ? null : JToken.FromObject(val);
    }
}
