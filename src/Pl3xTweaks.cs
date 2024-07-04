using System.Reflection;
using HarmonyLib;
using pl3xtweaks.module;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.GameContent;
using Vintagestory.Server;
using Module = pl3xtweaks.module.Module;

namespace pl3xtweaks;

// ReSharper disable once ClassNeverInstantiated.Global
// ReSharper disable once InconsistentNaming
public sealed class Pl3xTweaks : ModSystem {
    private const BindingFlags _flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

    public ICoreClientAPI Api { get; private set; } = null!;
    public string ModId => Mod.Info.ModID;

    private readonly List<Module> _modules = new();

    private Harmony? _harmony;

    public override void StartPre(ICoreAPI api) {
        if (api is ICoreClientAPI capi) {
            Api = capi;
        }

        _harmony = new Harmony(Mod.Info.ModID);

        ItemChisel.carvingTime = true;

        _modules.Add(new BetterPropick(this));
        _modules.Add(new BlockParticles(this));
        //_modules.Add(new Buzzwords(this)); // todo - needs toggle
        _modules.Add(new ClimbableTrapdoors(this));
        _modules.Add(new CreatureKilledBy(this));
        _modules.Add(new FixDanasShit(this));
        _modules.Add(new IngotMoldBoxes(this));
        _modules.Add(new NoSleepSkipNight(this));
        _modules.Add(new RememberWaypointNames(this));
        _modules.Add(new ShowChunksWireFrame(this));
        _modules.Add(new Shutdown(this));
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
        }
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
        foreach (Module module in _modules) {
            module.Dispose();
        }

        _modules.Clear();

        _harmony?.UnpatchAll(Mod.Info.ModID);
    }
}
