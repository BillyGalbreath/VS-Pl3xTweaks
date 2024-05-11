using System.Reflection;

namespace Pl3xTweaks.Extensions;

public static class ReflectionExtensions {
    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

    public static T? GetField<T>(this object obj, string name) {
        return (T?)obj.GetType().GetField(name, Flags)?.GetValue(obj);
    }
}
