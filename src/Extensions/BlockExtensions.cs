using Newtonsoft.Json.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace Pl3xTweaks.Extensions;

public static class BlockExtensions {
    public static void SetAttributeToken(this CollectibleObject obj, string attr, object? val) {
        (obj.Attributes ??= new JsonObject(new JObject()))
            .Token[attr] = val == null ? null : JToken.FromObject(val);
    }
}
