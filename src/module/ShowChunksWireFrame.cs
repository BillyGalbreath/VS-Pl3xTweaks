using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace Pl3xTweaks.module;

public class ShowChunksWireFrame : Module {
    public ShowChunksWireFrame(ICoreClientAPI api) {
        api.ChatCommands.Create("wireframe")
            .WithDescription("Shows the chunk wireframe")
            .WithRootAlias("showchunks")
            .HandleWith(_ => TextCommandResult.Success(Lang.Get("Chunk wireframe now {0}",
                api.ToggleWireframe() ? Lang.Get("on") : Lang.Get("off"))));
    }
}
