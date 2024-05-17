using pl3xtweaks.client.patches;
using pl3xtweaks.common;
using pl3xtweaks.common.patches;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace pl3xtweaks.client;

public sealed class ClientTweaks : AbstractTweaks {
    public ClientTweaks(TweaksMod mod, ICoreClientAPI api) : base(mod) {
        api.ChatCommands.Create("wireframe")
            .WithDescription("Shows the chunk wireframe")
            .WithRootAlias("showchunks")
            .HandleWith(_ => TextCommandResult.Success(Lang.Get("Chunk wireframe now {0}",
                api.ToggleWireframe() ? Lang.Get("on") : Lang.Get("off"))));

        _ = new BlockIngotMoldPatches(Harmony);
        _ = new GameCalendarPatches(Harmony);
        _ = new SystemTemporalStabilityPatches(Harmony);
    }
}
