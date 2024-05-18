using System;
using pl3xtweaks;
using Vintagestory.API.Client;
using Vintagestory.Client.NoObf;

namespace Pl3xTweaks;

public static class Extensions {
    private static readonly PluralFormatProvider PluralFormatProvider = new();

    public static T? GetField<T>(this object obj, string name) {
        return (T?)obj.GetType().GetField(name, TweaksMod.Flags)?.GetValue(obj);
    }

    public static bool ToggleWireframe(this ICoreClientAPI api) {
        ClientMain game = (ClientMain)api.World;
        return game.ChunkWireframe = !game.ChunkWireframe;
    }

    public static string Format(this string format, params object?[] args) {
        return string.Format(PluralFormatProvider, format, args);
    }
}

public class PluralFormatProvider : IFormatProvider, ICustomFormatter {
    public object GetFormat(Type? formatType) {
        return this;
    }

    public string Format(string? format, object? arg, IFormatProvider? formatProvider) {
        if (format == null) {
            return $"{arg}";
        }

        if (!format.Contains(';')) {
            return string.Format($"{{0:{format}}}", arg);
        }

        int i = (int)(arg ?? 0) == 1 ? 0 : 1;
        return $"{arg} {format.Split(';')[i]}";
    }
}
