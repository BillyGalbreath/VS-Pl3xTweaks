using Vintagestory.API.Client;

namespace pl3xtweaks.module;

public class Shutdown : Module {
    private readonly ICoreClientAPI _api;

    public Shutdown(Pl3xTweaks mod, ICoreClientAPI api) {
        _api = api;

        string channelName = $"{mod.Mod.Info.ModID}:shutdown";

        api.Network.RegisterChannel(channelName)
            .RegisterMessageType<MessagePacket>()
            .SetMessageHandler<MessagePacket>(Receive);
    }

    private void Receive(MessagePacket packet) {
        if (!string.IsNullOrEmpty(packet.Message)) {
            _api.TriggerIngameError(this, "error", packet.Message);
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private class MessagePacket {
        public string? Message { get; }

        public MessagePacket(string message) {
            Message = message;
        }
    }
}
