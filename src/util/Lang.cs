namespace pl3xtweaks.util;

public abstract class Lang {
    public static string Get(string key, params object[]? args) {
        return Vintagestory.API.Config.Lang.Get(key.Contains(':') ? key : $"pl3xtweaks:{key}", args);
    }

    public static string GetL(string langCode, string key, params object[]? args) {
        return Vintagestory.API.Config.Lang.GetL(langCode, key.Contains(':') ? key : $"pl3xtweaks:{key}", args);
    }

    public static string Error(string key, params object[]? args) {
        return Get("error-format", Get(key, args));
    }

    public static string Success(string key, params object[]? args) {
        return Get("success-format", Get(key, args));
    }
}
