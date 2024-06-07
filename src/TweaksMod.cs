using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using HarmonyLib;
using Pl3xTweaks.block.trashcan;
using Pl3xTweaks.configuration;
using Pl3xTweaks.module;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.GameContent;
using Module = Pl3xTweaks.module.Module;

namespace pl3xtweaks;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public sealed class TweaksMod : ModSystem {
    public const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

    private static TweaksMod Instance { get; set; } = null!;

    public static ICoreAPI? Api => Instance._api;
    public static ILogger Logger => Instance.Mod.Logger;
    public static string Id => Instance.Mod.Info.ModID;

    private readonly List<Module> _modules = new();

    private ICoreAPI? _api;
    private Harmony? _harmony;

    public TweaksMod() {
        Instance = this;
    }

    public override void Start(ICoreAPI api) {
        _api = api;

        _harmony = new Harmony(Mod.Info.ModID);

        // todo = move this to json patch??
        ItemChisel.carvingTime = true;

        api.RegisterBlockClass("trashcan", typeof(TrashcanBlock));
        api.RegisterBlockEntityClass("betrashcan", typeof(BETrashcan));
    }

    public override void StartClientSide(ICoreClientAPI api) {
        _modules.Add(new ClimbableTrapdoors(api));
        _modules.Add(new CreatureKilledBy(this));
        _modules.Add(new IngotMoldBoxes(this));
        _modules.Add(new NoSleepSkipNight(this));
        _modules.Add(new NoSurfaceInstability(this));
        _modules.Add(new RememberWaypointNames(this));
        _modules.Add(new ShowChunksWireFrame(api));
        _modules.Add(new Shutdown(this, api));
    }

    public override void StartServerSide(ICoreServerAPI api) {
        //_modules.Add(new BackOnDeath(api));
        _modules.Add(new BedRespawn(api));
        _modules.Add(new BetterFirepit(this));
        _modules.Add(new BroadcastTips(api));
        _modules.Add(new ClimbableTrapdoors(api));
        _modules.Add(new CooperativeCombat(this));
        _modules.Add(new DeathMessageFix(this));
        _modules.Add(new ExtendedPickupReach(this));
        _modules.Add(new FirstJoin(this));
        _modules.Add(new NextTempStorm(api));
        _modules.Add(new NoOffhandHunger(api));
        _modules.Add(new NoSleepSkipNight(this));
        _modules.Add(new NoSurfaceInstability(this));
        _modules.Add(new PitKilnIgniteNeighbors(this));
        _modules.Add(new PlayerChat(api));
        _modules.Add(new ServerHeartbeat(this));
        _modules.Add(new Shutdown(this, api));
    }

    public void Patch<T>(string original, Delegate? prefix = null, Delegate? postfix = null, Delegate? transpiler = null, Delegate? finalizer = null, Type[]? types = null) {
        if (_harmony == null) {
            throw new InvalidOperationException("Harmony has not been instantiated yet!");
        }
        MethodInfo? method = types == null ? typeof(T).GetMethod(original, Flags) : typeof(T).GetMethod(original, Flags, types);
        Patch(method, prefix, postfix, transpiler, finalizer);
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

        _harmony?.UnpatchAll(Mod.Info.ModID);

        Config.Dispose();
    }
}
