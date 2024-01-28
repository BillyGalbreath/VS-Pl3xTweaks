using Vintagestory.API.Client;
using Vintagestory.Client.NoObf;

namespace Pl3xTweaks.Extensions;

public static class ClientMainExtensions {
    public static bool ToggleWireframe(this ICoreClientAPI api) {
        ClientMain game = (ClientMain)api.World;
        return game.ChunkWireframe = !game.ChunkWireframe;
    }
}
