using System.Reflection;

namespace pl3xtweaks.util;

public static class Extensions {
    private const BindingFlags _flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;

    public static T? GetField<T>(this object obj, string name) where T : class {
        return obj.GetType().GetField(name, _flags)?.GetValue(obj) as T;
    }
}
