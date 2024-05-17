using pl3xtweaks.common;
using pl3xtweaks.common.patches;
using pl3xtweaks.server.command;
using pl3xtweaks.server.configuration;
using Pl3xTweaks.server.module;
using pl3xtweaks.server.patches;
using Vintagestory.API.Server;

namespace pl3xtweaks.server;

public class ServerTweaks : AbstractTweaks {
    private readonly BedRespawn _bedRespawn;
    private readonly BroadcastTips _broadcastTips;
    private readonly NoOffhandHunger _noOffhandHunger;
    private readonly PlayerChat _playerChat;

    public ServerTweaks(TweaksMod mod, ICoreServerAPI api) : base(mod) {
        // modules
        _bedRespawn = new BedRespawn(api);
        _broadcastTips = new BroadcastTips(api);
        _noOffhandHunger = new NoOffhandHunger(api);
        _playerChat = new PlayerChat(api);

        // commands
        _ = new NextTempStorm(api);

        // patches
        _ = new BlockEntityPitKilnPatches(Harmony);
        _ = new GameCalendarPatches(Harmony);
        _ = new EntityAgentPatches(Harmony);
        _ = new EntityBehaviorCollectEntitiesPatches(Harmony);
        _ = new FirepitPatches(Harmony);
        _ = new SystemTemporalStabilityPatches(Harmony);
    }

    public override void Dispose() {
        base.Dispose();

        _bedRespawn.Dispose();
        _broadcastTips.Dispose();
        _noOffhandHunger.Dispose();
        _playerChat.Dispose();

        Config.Dispose();
    }
}
