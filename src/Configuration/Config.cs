using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using Vintagestory.API.Config;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Pl3xTweaks.Configuration;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
public sealed class Config {
    private static readonly string ConfigFile = Path.Combine(GamePaths.ModConfig, $"{TweaksMod.Instance.Mod.Info.ModID}.yml");

    public static int Radius => Get.Radius;
    public static int Height => Get.Height;
    public static float BuildEffort => Get.BuildEffort;
    public static bool RequireFloor => Get.RequireFloor;
    public static bool ReplacePlantsAndRocks => Get.ReplacePlantsAndRocks;

    [SuppressMessage("ReSharper", "ConvertToConstant.Local")]
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
    [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
    private sealed class Data {
        [YamlMember] public int Radius = 3;
        [YamlMember] public int Height = 7;
        [YamlMember] public float BuildEffort = 100F;
        [YamlMember] public bool RequireFloor = false;
        [YamlMember] public bool ReplacePlantsAndRocks = true;
    }

    private static Data? _data;

    private static Data Get => _data ??= Write(Read());

    public static void Reload() {
        _data = Write(Read());
    }

    private static Data Read() {
        try {
            return new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .WithNamingConvention(NullNamingConvention.Instance)
                .Build().Deserialize<Data>(File.ReadAllText(ConfigFile));
        } catch (Exception) {
            return new Data();
        }
    }

    private static Data Write(Data data) {
        GamePaths.EnsurePathExists(GamePaths.ModConfig);
        File.WriteAllText(ConfigFile,
            new SerializerBuilder()
                .WithQuotingNecessaryStrings()
                .WithNamingConvention(NullNamingConvention.Instance)
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
                .Build().Serialize(data)
            , Encoding.UTF8);
        return data;
    }
}
