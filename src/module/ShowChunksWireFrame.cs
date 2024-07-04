using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.Client.NoObf;

namespace pl3xtweaks.module;

public class ShowChunksWireFrame : Module {
    public ShowChunksWireFrame(Pl3xTweaks mod) : base(mod) { }

    public override void StartClientSide(ICoreClientAPI api) {
        _mod.Api.ChatCommands.Create("wireframe")
            .WithDescription("Shows the chunk wireframe")
            .WithRootAlias("showchunks")
            .HandleWith(_ => {
                string onoff = ToggleWireframe(_mod.Api) ? Lang.Get("on") : Lang.Get("off");
                string message = Lang.Get("Chunk wireframe now {0}", onoff);
                return TextCommandResult.Success(message);
            });
    }

    private static bool ToggleWireframe(ICoreClientAPI api) {
        ClientMain game = (ClientMain)api.World;
        return game.ChunkWireframe = !game.ChunkWireframe;
    }
}
