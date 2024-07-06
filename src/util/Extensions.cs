using System.Reflection;

namespace pl3xtweaks.util;

public static class Extensions {
    private const BindingFlags _flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;

    public static T? GetField<T>(this object obj, string name) where T : class {
        return obj.GetType().GetField(name, _flags)?.GetValue(obj) as T;
    }

    public static void Invoke(this object obj, string name, object?[]? parameters = null) {
        obj.Invoke<object>(name, parameters);
    }

    public static T? Invoke<T>(this object obj, string name, object?[]? parameters = null) {
        return (T?)obj.GetType().GetMethod(name, _flags)?.Invoke(obj, parameters);
    }
}
