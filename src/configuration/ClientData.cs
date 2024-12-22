using Vintagestory.API.Client;

namespace pl3xtweaks.configuration;

public class ClientData {
    private readonly string _filename;
    private readonly ICoreClientAPI _capi;

    public ClientData(Pl3xTweaks mod, ICoreClientAPI api) {
        _filename = $"{mod.ModId}client.json";
        _capi = api;
    }

    public void SaveData(string key, object? value) {
        Dictionary<string, object?> data = GetData();
        data[key] = value;
        _capi.Logger.Error($"############ SAVING {key}: {value}");
        _capi.StoreModConfig(data, _filename);
    }

    public T? GetData<T>(string key, object? def = null) {
        Dictionary<string, object?> data = GetData();
        T? value = (T?)(data.GetValueOrDefault(key, def) ?? default(T));
        _capi.Logger.Error($"############ GETTING {key}: {value}");
        return value;
    }

    private Dictionary<string, object?> GetData() {
        return _capi.LoadModConfig<Dictionary<string, object?>>(_filename) ?? new Dictionary<string, object?>();
    }
}
