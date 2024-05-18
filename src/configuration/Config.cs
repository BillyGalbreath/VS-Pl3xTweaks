using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using pl3xtweaks;
using Vintagestory.API.Config;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Pl3xTweaks.configuration;

//
// todo replace this whole system
//

[SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class Config {
    [YamlMember(Order = 0, Description = null)]
    public TipsConfig Tips = new();

    public static TipsConfig TipsConfig => Get().Tips;

    private static string ConfigFile => Path.Combine(GamePaths.ModConfig, $"{TweaksMod.Id}.yml");

    private static FileWatcher? _watcher;
    private static Config? _config;

    private static Config Get() {
        return _config ??= Reload();
    }

    public static Config Reload() {
        _config = Write(Read());

        _watcher ??= new FileWatcher(TweaksMod.Api!);

        TipsConfig.Reset();

        return _config;
    }

    private static Config Read() {
        try {
            string yaml = File.ReadAllText(ConfigFile, Encoding.UTF8);
            return new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .WithNamingConvention(NullNamingConvention.Instance)
                .Build().Deserialize<Config>(yaml);
        } catch (Exception) {
            return new Config();
        }
    }

    private static Config Write(Config config) {
        GamePaths.EnsurePathExists(GamePaths.ModConfig);
        string yaml = new SerializerBuilder()
            .WithQuotingNecessaryStrings()
            .WithNamingConvention(NullNamingConvention.Instance)
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
            .Build().Serialize(config);
        File.WriteAllText(ConfigFile, yaml, Encoding.UTF8);
        return config;
    }

    public static void Dispose() {
        _watcher?.Dispose();
        _watcher = null;
    }
}
