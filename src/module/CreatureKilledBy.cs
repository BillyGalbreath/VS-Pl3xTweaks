using System.Reflection;
using Vintagestory.API.Client;
using Vintagestory.API.Config;
using Vintagestory.GameContent;

namespace pl3xtweaks.module;

public class CreatureKilledBy : Module {
    private static string? _killedBy;

    public CreatureKilledBy(Pl3xTweaks mod) : base(mod) { }

    public override void StartClientSide(ICoreClientAPI api) {
        _mod.Patch<EntityBehaviorHarvestable>("GetInfoText", prefix: Pre, postfix: Post);
        _mod.Patch(typeof(Lang).GetMethod("Get", BindingFlags.Static | BindingFlags.Public), AddKilledBy);
    }

    private static void Pre(EntityBehaviorHarvestable __instance) {
        _killedBy = __instance.entity.WatchedAttributes.GetString("deathByEntity")?.Split(":")[1];
    }

    private static void Post() {
        _killedBy = null;
    }

    private static bool AddKilledBy(ref string __result, string key, params object[]? args) {
        if (_killedBy == null || args == null || args.Length > 0) {
            return true;
        }

        if (key.StartsWith("Looks crushed") || key.StartsWith("Looks partially charred") || key.StartsWith("creature-weight")) {
            return true;
        }

        // don't overflow :3
        string killedBy = _killedBy;
        _killedBy = null;

        __result = $"{Lang.Get(key)} [{Lang.Get($"item-creature-{killedBy}")}]";
        return false;
    }

    public override void Dispose() {
        base.Dispose();
        _killedBy = null;
    }
}
