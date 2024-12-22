using System.Diagnostics.CodeAnalysis;

namespace pl3xtweaks.configuration;

[SuppressMessage("ReSharper", "CollectionNeverUpdated.Global")]
[SuppressMessage("ReSharper", "ConvertToConstant.Global")]
[SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
public class TipsData {
    public int Interval = 3600000; // 1 hour in millis

    public string Prefix = "[Tip]: {0}";

    public List<string> List = new();
}
