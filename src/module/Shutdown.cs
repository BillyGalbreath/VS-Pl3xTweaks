using System.Diagnostics.CodeAnalysis;
using Vintagestory.API.Client;

namespace pl3xtweaks.module;

public class Shutdown : Module {
    public Shutdown(Pl3xTweaks mod) : base(mod) { }

    public override void StartClientSide(ICoreClientAPI api) {
        const string channelName = "servertweaks:shutdown";

        _mod.Api.Network.RegisterChannel(channelName)
            .RegisterMessageType<MessagePacket>()
            .SetMessageHandler<MessagePacket>(Receive);
    }

    private void Receive(MessagePacket packet) {
        if (!string.IsNullOrEmpty(packet.Message)) {
            _mod.Api.TriggerIngameError(this, "error", packet.Message);
        }
    }

    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
    private class MessagePacket {
        public string? Message { get; }

        public MessagePacket(string message) {
            Message = message;
        }
    }
}
