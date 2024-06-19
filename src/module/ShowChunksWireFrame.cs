using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace pl3xtweaks.module;

public class ShowChunksWireFrame : Module {
    public ShowChunksWireFrame(ICoreClientAPI api) {
        api.ChatCommands.Create("wireframe")
            .WithDescription("Shows the chunk wireframe")
            .WithRootAlias("showchunks")
            .HandleWith(_ => {
                string onoff = api.ToggleWireframe() ? Lang.Get("on") : Lang.Get("off");
                string message = Lang.Get("Chunk wireframe now {0}", onoff);
                return TextCommandResult.Success(message);
            });
    }
}
