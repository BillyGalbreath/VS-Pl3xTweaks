using pl3xtweaks.util;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace pl3xtweaks.module;

public class ShowChunksWireFrame : Module {
    public ShowChunksWireFrame(Pl3xTweaks mod) : base(mod) { }

    public override void StartClientSide(ICoreClientAPI api) {
        api.ChatCommands.Create("wireframe")
            .WithDescription("Shows the chunk wireframe")
            .WithRootAlias("showchunks")
            .HandleWith(_ => {
                string onoff = ToggleWireframe(api) ? Lang.Get("game:on") : Lang.Get("game:off");
                string message = Lang.Get("game:Chunk wireframe now {0}", onoff);
                return TextCommandResult.Success(message);
            });
    }

    private static bool ToggleWireframe(ICoreClientAPI api) {
        return api.Render.WireframeDebugRender.Chunk = !api.Render.WireframeDebugRender.Chunk;
    }
}
