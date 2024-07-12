using System.Reflection;
using HarmonyLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using pl3xtweaks.configuration;
using pl3xtweaks.module;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Vintagestory.GameContent;
using Vintagestory.Server;
using Module = pl3xtweaks.module.Module;
using Tips = pl3xtweaks.module.Tips;

namespace pl3xtweaks;

// ReSharper disable once ClassNeverInstantiated.Global
// ReSharper disable once InconsistentNaming
public sealed class Pl3xTweaks : ModSystem {
    private const BindingFlags _flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

    public Config Config { get; private set; } = null!;
    public ILogger Logger => Mod.Logger;
    public string ModId => Mod.Info.ModID;

    private ICoreAPI? _api;
    private FileWatcher? _fileWatcher;
    private Harmony? _harmony;

    private readonly List<Module> _modules = new();

    public override void StartPre(ICoreAPI api) {
        _api = api;

        _harmony = new Harmony(Mod.Info.ModID);

        ItemChisel.carvingTime = true;

        _modules.Add(new AlwaysChiselPumpkin(this));
        _modules.Add(new BedRespawn(this));
        _modules.Add(new BetterFirepit(this));
        _modules.Add(new BetterPropick(this));
        _modules.Add(new BlockParticles(this));
        _modules.Add(new BodyHeatBar(this));
        //_modules.Add(new Buzzwords(this)); // todo - needs toggle
        _modules.Add(new CanSleepAtAnyTime(this));
        _modules.Add(new ClimbableTrapdoors(this));
        _modules.Add(new CooperativeCombat(this));
        _modules.Add(new CreatureKilledBy(this));
        _modules.Add(new DeathMessageFix(this));
        _modules.Add(new FirstJoinMessage(this));
        _modules.Add(new FixDanasShit(this));
        _modules.Add(new GlowingProjectiles(this));
        _modules.Add(new HealthBarOverlay(this));
        _modules.Add(new IngotMoldBoxes(this));
        _modules.Add(new ItemInChat(this));
        _modules.Add(new Kits(this));
        _modules.Add(new LabeledChestGiveBack(this));
        _modules.Add(new NextTempStorm(this));
        _modules.Add(new NoOffhandHunger(this));
        _modules.Add(new NoSleepSkipNight(this));
        _modules.Add(new PitKilnIgniteNeighbors(this));
        _modules.Add(new RememberWaypointNames(this));
        _modules.Add(new ServerNameAndDescription(this));
        _modules.Add(new ShowChunksWireFrame(this));
        _modules.Add(new Shutdown(this));
        _modules.Add(new Tips(this));
        _modules.Add(new Trashcan(this));

        //_modules.ForEach(module => module.StartPre(api));
    }

    public override void Start(ICoreAPI api) {
        _modules.ForEach(module => module.Start(api));
    }

    public override void AssetsLoaded(ICoreAPI api) {
        //_modules.ForEach(module => module.AssetsLoaded(api));
    }

    public override void AssetsFinalize(ICoreAPI api) {
        _modules.ForEach(module => module.AssetsFinalize(api));
    }

    public override void StartClientSide(ICoreClientAPI api) {
        _modules.ForEach(module => module.StartClientSide(api));
    }

    public override void StartServerSide(ICoreServerAPI api) {
        if (!((ServerMain)api.World).RawCmdLineArgs.Contains("thismodisforpl3xserveronlylol")) {
            api.Event.PlayerJoin += player => player.Disconnect();
            Dispose();
            return;
        }

        ReloadConfig(api);

        _modules.ForEach(module => module.StartServerSide(api));

        api.Event.SaveGameLoaded += OnSaveGameLoaded;
        api.Event.GameWorldSave += OnGameWorldSave;
    }

    private void OnGameWorldSave() {
        _modules.ForEach(module => module.OnGameWorldSave());
    }

    private void OnSaveGameLoaded() {
        _modules.ForEach(module => module.OnSaveGameLoaded());
    }

    public void ReloadConfig(ICoreServerAPI api) {
        GamePaths.EnsurePathExists(GamePaths.ModConfig);

        Config = api.LoadModConfig<Config>($"{ModId}.json") ?? new Config();

        string json = JsonConvert.SerializeObject(Config, new JsonSerializerSettings {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Include,
            DefaultValueHandling = DefaultValueHandling.Include,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        });

        (_fileWatcher ??= new FileWatcher(this, api)).Queued = true;

        File.WriteAllText(Path.Combine(GamePaths.ModConfig, $"{ModId}.json"), json);

        _modules.ForEach(module => module.Reload());

        api.Event.RegisterCallback(_ => _fileWatcher.Queued = false, 100);
    }

    public void Patch<T>(string original, Delegate? prefix = null, Delegate? postfix = null, Delegate? transpiler = null, Delegate? finalizer = null, Type[]? types = null) {
        if (_harmony == null) {
            throw new InvalidOperationException("Harmony has not been instantiated yet!");
        }
        MethodInfo? method = types == null ? typeof(T).GetMethod(original, _flags) : typeof(T).GetMethod(original, _flags, types);
        Patch(method, prefix, postfix, transpiler, finalizer);
    }

    public void Patch(string typeName, string methodName, Delegate? prefix = null, Delegate? postfix = null, Delegate? transpiler = null, Delegate? finalizer = null) {
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
            foreach (Type type in assembly.GetTypes()) {
                if ((type.FullName ?? "").Equals(typeName)) {
                    MethodBase? method = type.GetMethod(methodName, _flags) ?? type.GetProperty(methodName, _flags)?.GetGetMethod();
                    Patch(method, prefix, postfix, transpiler, finalizer);
                    return;
                }
            }
        }
    }

    public void Patch(MethodBase? method, Delegate? prefix = null, Delegate? postfix = null, Delegate? transpiler = null, Delegate? finalizer = null) {
        if (_harmony == null) {
            throw new InvalidOperationException("Harmony has not been instantiated yet!");
        }
        if (prefix != null) {
            _harmony.Patch(method, prefix: prefix);
        }

        if (postfix != null) {
            _harmony.Patch(method, postfix: postfix);
        }

        if (transpiler != null) {
            _harmony.Patch(method, transpiler: transpiler);
        }

        if (finalizer != null) {
            _harmony.Patch(method, finalizer: finalizer);
        }
    }

    public override void Dispose() {
        _fileWatcher?.Dispose();
        _fileWatcher = null;

        foreach (Module module in _modules) {
            module.Dispose();
        }
        _modules.Clear();

        if (_api is ICoreServerAPI sapi) {
            sapi.Event.SaveGameLoaded -= OnSaveGameLoaded;
            sapi.Event.GameWorldSave -= OnGameWorldSave;
        }

        Config = null!;

        _harmony?.UnpatchAll(Mod.Info.ModID);
    }
}
