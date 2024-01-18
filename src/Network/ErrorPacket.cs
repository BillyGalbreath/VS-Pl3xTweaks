using ProtoBuf;

namespace Pl3xTweaks.Network;

[ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
public sealed class ErrorPacket : Packet {
    public string? Error = null;
}
