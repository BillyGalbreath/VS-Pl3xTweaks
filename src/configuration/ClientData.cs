using Vintagestory.API.Client;

namespace pl3xtweaks.configuration;

public class ClientData(Pl3xTweaks __mod, ICoreClientAPI __api) {
    private readonly string _filename = $"{__mod.ModId}client.json";

    public void SaveData(string key, object? value) {
        Dictionary<string, object?> data = GetData();
        data[key] = value;
        __api.StoreModConfig(data, _filename);
    }

    public T? GetData<T>(string key, object? def = null) {
        Dictionary<string, object?> data = GetData();
        T? value = (T?)(data.GetValueOrDefault(key, def) ?? default(T));
        return value;
    }

    private Dictionary<string, object?> GetData() {
        return __api.LoadModConfig<Dictionary<string, object?>>(_filename) ?? new Dictionary<string, object?>();
    }
}
