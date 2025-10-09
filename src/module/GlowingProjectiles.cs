using pl3xtweaks.util;
using Vintagestory.API.Client;
using Vintagestory.API.Common.Entities;
using Vintagestory.Common;
using Vintagestory.GameContent;

namespace pl3xtweaks.module;

public class GlowingProjectiles(Pl3xTweaks __mod) : Module(__mod) {
    public override void StartClientSide(ICoreClientAPI api) {
        api.Event.RegisterCallback(_ => {
            Dictionary<string, Type?>? mappings = ((ClassRegistryAPI)api.ClassRegistry).GetField<ClassRegistry>("registry")?.entityClassNameToTypeMapping;
            foreach (EntityProperties properties in api.World.EntityTypes.Where(properties => IsProjectile(mappings, properties))) {
                properties.Client.GlowLevel = 0xFF;
            }
        }, 1000);
    }

    private static bool IsProjectile(Dictionary<string, Type?>? mappings, EntityProperties properties) {
        if ((mappings?.TryGetValue(properties.Class, out Type? type) ?? false) &&
            (type?.IsAssignableFrom(typeof(EntityProjectile)) ?? false)) {
            return true;
        }

        if (properties.Class.Contains("projectile", StringComparison.CurrentCultureIgnoreCase)) {
            return true;
        }

        return properties.Code?.ToString().Contains("projectile", StringComparison.CurrentCultureIgnoreCase) ?? false;
    }
}
