using System.Reflection;

namespace Pl3xTweaks.Extensions;

public static class ReflectionExtensions {
    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

    public static T? GetField<T>(this object obj, string name) {
        return (T?)obj.GetType().GetField(name, Flags)?.GetValue(obj);
    }

    public static void SetField(this object obj, string name, object? val) {
        obj.GetType().GetField(name, Flags)?.SetValue(obj, val);
    }

    public static void Invoke(this object obj, string name, object?[]? args) {
        obj.GetType().GetMethod(name, Flags)?.Invoke(obj, args);
    }
}
