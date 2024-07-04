using pl3xtweaks.util;
using Vintagestory.API.Client;
using Vintagestory.API.Common.Entities;
using Vintagestory.Common;
using Vintagestory.GameContent;

namespace pl3xtweaks.module;

public class GlowingProjectiles : Module {
    public GlowingProjectiles(Pl3xTweaks mod) : base(mod) { }

    public override void StartClientSide(ICoreClientAPI api) {
        api.Event.RegisterCallback(_ => {
            Dictionary<string, Type?>? mappings = ((ClassRegistryAPI)api.ClassRegistry).GetField<ClassRegistry>("registry")?.entityClassNameToTypeMapping;
            foreach (EntityProperties properties in api.World.EntityTypes.Where(properties => IsProjectile(mappings, properties))) {
                properties.Client.GlowLevel = 0xFF;
            }
        }, 1000);
    }

    private static bool IsProjectile(IReadOnlyDictionary<string, Type?>? mappings, EntityProperties properties) {
        if ((mappings?.TryGetValue(properties.Class, out Type? type) ?? false) &&
            (type?.IsAssignableFrom(typeof(EntityProjectile)) ?? false)) {
            return true;
        }

        if (properties.Class.ToLower().Contains("projectile")) {
            return true;
        }

        return properties.Code?.ToString().ToLower().Contains("projectile") ?? false;
    }
}
