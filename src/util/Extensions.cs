using Vintagestory.API.Client;
using Vintagestory.Client.NoObf;

namespace pl3xtweaks.util;

public static class Extensions {
    public static bool ToggleWireframe(this ICoreClientAPI api) {
        ClientMain game = (ClientMain)api.World;
        return game.ChunkWireframe = !game.ChunkWireframe;
    }
}
