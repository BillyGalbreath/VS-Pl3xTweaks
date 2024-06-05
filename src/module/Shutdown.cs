using System;
using System.Linq;
using pl3xtweaks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.Server;

namespace Pl3xTweaks.module;

public class Shutdown : Module {
    private readonly ILogger _logger;
    private readonly ICoreAPI _api;

    private readonly DateTime _shutdown;
    private readonly long _listenerId;

    private IServerNetworkChannel? _serverChannel;

    private bool _warning15;
    private bool _warning10;
    private bool _warning5;
    private bool _warning2;
    private bool _warning1;

    public Shutdown(TweaksMod mod, ICoreAPI api) {
        _logger = mod.Mod.Logger;
        _api = api;

        string channelName = $"{mod.Mod.Info.ModID}:shutdown";

        if (api is ICoreClientAPI capi) {
            capi.Network.RegisterChannel(channelName)
                .RegisterMessageType<MessagePacket>()
                .SetMessageHandler<MessagePacket>(Receive);
        }

        if (api is not ICoreServerAPI sapi) {
            return;
        }

        _serverChannel = sapi.Network.RegisterChannel(channelName)
            .RegisterMessageType<MessagePacket>();

        _shutdown = GetNextShutdown();

        _listenerId = api.Event.RegisterGameTickListener(Tick, 1000, 1000);
    }

    private void Tick(float delta) {
        int remaining = (int)(_shutdown - DateTime.Now).TotalSeconds;

        // ReSharper disable once ConvertIfStatementToSwitchStatement
        if (remaining > 15 * 60) {
            // short circuit
            return;
        }

        if (remaining <= 0) {
            ServerMain server = ((ICoreServerAPI)_api).GetField<ServerMain>("server")!;
            _logger.Event("AUTOMATIC SHUTDOWN!");
            _api.World.AllOnlinePlayers.Cast<IServerPlayer>()
                .Foreach(player => {
                    ConnectedClient client = ((ServerPlayer)player).GetField<ConnectedClient>("client")!;
                    server.DisconnectPlayer(client, null, "Server is restarting..");
                });
            (_api as ICoreServerAPI)?.Server.ShutDown();
        } else if (remaining <= 1 * 60) {
            if (!_warning1) {
                _warning1 = true;
                Broadcast("Server restarting in 60 seconds");
            }
            MessagePacket packet = new($"Server restarting in {remaining} second{(remaining == 1 ? "" : "s")}");
            _api.World.AllOnlinePlayers.Cast<IServerPlayer>()
                .Foreach(player => _serverChannel?.SendPacket(packet, player));
        } else if (remaining <= 2 * 60) {
            if (!_warning2) {
                _warning2 = true;
                Broadcast("Server restarting in 2 minutes");
            }
        } else if (remaining <= 5 * 60) {
            if (!_warning5) {
                _warning5 = true;
                Broadcast("Server restarting in 5 minutes");
            }
        } else if (remaining <= 10 * 60) {
            if (!_warning10) {
                _warning10 = true;
                Broadcast("Server restarting in 10 minutes");
            }
        } else {
            if (!_warning15) {
                _warning15 = true;
                Broadcast("Server restarting in 15 minutes");
            }
        }
    }

    private void Broadcast(string message) {
        _logger.Event(message);
        _api.World.AllOnlinePlayers.Cast<IServerPlayer>()
            .Foreach(player => player.SendMessage(GlobalConstants.AllChatGroups, message, EnumChatType.Notification));
    }

    private void Receive(MessagePacket packet) {
        if (!string.IsNullOrEmpty(packet.Message)) {
            (_api as ICoreClientAPI)?.TriggerIngameError(this, "error", packet.Message);
        }
    }

    private static DateTime GetNextShutdown() {
        DateTime now = DateTime.Now;
        DateTime am = Next(now, TimeOnly.Parse("03:00"));
        DateTime pm = Next(now, TimeOnly.Parse("15:00"));
        return (am - now).TotalSeconds < (pm - now).TotalSeconds ? am : pm;
    }

    private static DateTime Next(DateTime now, TimeOnly time) {
        DateTime today = now.Date + time.ToTimeSpan();
        return (now <= today) ? today : today.AddDays(1);
    }

    public override void Dispose() {
        base.Dispose();

        if (_api is ICoreServerAPI sapi) {
            _serverChannel = null;
            sapi.Event.UnregisterGameTickListener(_listenerId);
        }
    }

    private class MessagePacket {
        public string? Message { get; }

        public MessagePacket(string message) {
            Message = message;
        }
    }
}
