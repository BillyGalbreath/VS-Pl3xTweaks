using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.Server;

namespace pl3xtweaks.module;

public class Shutdown : Module {
    private readonly string _channelName;

    private IServerNetworkChannel? _serverChannel;
    private ICoreServerAPI? _sapi;
    private DateTime _shutdown;
    private long _listenerId;

    private bool _warning15;
    private bool _warning10;
    private bool _warning5;
    private bool _warning2;
    private bool _warning1;

    public Shutdown(Pl3xTweaks mod) : base(mod) {
        _channelName = $"{mod.ModId}:shutdown";
    }

    public override void StartClientSide(ICoreClientAPI api) {
        api.Network.RegisterChannel(_channelName)
            .RegisterMessageType<MessagePacket>()
            .SetMessageHandler<MessagePacket>(packet => Receive(api, packet));
    }

    public override void StartServerSide(ICoreServerAPI api) {
        _sapi = api;
        _serverChannel = api.Network.RegisterChannel(_channelName)
            .RegisterMessageType<MessagePacket>();
        _shutdown = GetNextShutdown();
        _listenerId = api.Event.RegisterGameTickListener(Tick, 1000, 1000);
    }

    private void Broadcast(string message) {
        _mod.Logger.Event(message);
        _sapi!.World.AllOnlinePlayers.Cast<IServerPlayer>()
            .Foreach(player => player.SendMessage(GlobalConstants.AllChatGroups, message, EnumChatType.Notification));
    }

    private void Receive(ICoreClientAPI api, MessagePacket packet) {
        if (!string.IsNullOrEmpty(packet.Message)) {
            api.TriggerIngameError(this, "error", packet.Message);
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

    private void Tick(float delta) {
        int remaining = (int)(_shutdown - DateTime.Now).TotalSeconds;

        // ReSharper disable once ConvertIfStatementToSwitchStatement
        if (remaining <= 0) {
            ServerMain server = (ServerMain)_sapi!.World;
            _mod.Logger.Event("AUTOMATIC SHUTDOWN!");
            _sapi.World.AllOnlinePlayers.Cast<IServerPlayer>()
                .Foreach(player => {
                    if (server.Clients.TryGetValue(player.ClientId, out ConnectedClient? client)) {
                        server.DisconnectPlayer(client, null, "Server is restarting..");
                    }
                });
            _sapi.Server.ShutDown();
        } else if (remaining <= 1 * 60) {
            if (!_warning1) {
                _warning1 = true;
                Broadcast("Server restarting in 60 seconds");
            }
            MessagePacket packet = new($"Server restarting in {remaining} second{(remaining == 1 ? "" : "s")}");
            _sapi!.World.AllOnlinePlayers.Cast<IServerPlayer>()
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
        } else if (remaining <= 15 * 60) {
            if (!_warning15) {
                _warning15 = true;
                Broadcast("Server restarting in 15 minutes");
            }
        }
    }

    public override void Dispose() {
        base.Dispose();
        _serverChannel = null;
        _sapi?.Event.UnregisterGameTickListener(_listenerId);
    }

    private class MessagePacket {
        public string? Message { get; }

        public MessagePacket(string message) {
            Message = message;
        }
    }
}
