using pl3xtweaks.util;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace pl3xtweaks.module;

public class ShowChunksWireFrame : Module {
    public ShowChunksWireFrame(Pl3xTweaks mod) : base(mod) {
        _mod.Api.ChatCommands.Create("wireframe")
            .WithDescription("Shows the chunk wireframe")
            .WithRootAlias("showchunks")
            .HandleWith(_ => {
                string onoff = _mod.Api.ToggleWireframe() ? Lang.Get("on") : Lang.Get("off");
                string message = Lang.Get("Chunk wireframe now {0}", onoff);
                return TextCommandResult.Success(message);
            });
    }
}
