using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Util;

namespace pl3xtweaks.module;

public class NoTraderClaims(Pl3xTweaks __mod) : Module(__mod) {
    public override void StartServerSide(ICoreServerAPI api) {
        api.Event.RegisterCallback(_ => {
            new List<LandClaim>(api.World.Claims.All ?? [])
                .Where(claim => claim is { OwnedByEntityId: 0, LastKnownOwnerName: "Trader" })
                .Foreach(claim => {
                    _mod.Logger.Event($"Removing trader claim at {claim.Center}");
                    api.World.Claims.Remove(claim);
                });
        }, 1);
    }
}
