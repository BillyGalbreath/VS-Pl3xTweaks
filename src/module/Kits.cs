using System.Text.RegularExpressions;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Lang = pl3xtweaks.util.Lang;

namespace pl3xtweaks.module;

public partial class Kits : Module {
    private const string _kitsKey = "pl3xtweaks:kits";

    [GeneratedRegex("^(?i)[a-z][a-z0-9]*$", RegexOptions.None, "en-US")]
    private static partial Regex ValidNameRegex();

    private ICoreServerAPI? _api;

    public Kits(Pl3xTweaks mod) : base(mod) { }

    public override void StartServerSide(ICoreServerAPI api) {
        _api = api;
        api.ChatCommands
            .Create("kit")
            .WithDescription(Lang.Get("kit-description"))
            .RequiresPlayer()
            .RequiresPrivilege(Privilege.chat)
            .WithArgs(new KitArgParser("name"))
            .HandleWith(CmdKit);
        api.ChatCommands.Create("setkit")
            .WithDescription(Lang.Get("setkit-description"))
            .RequiresPlayer()
            .RequiresPrivilege(Privilege.root)
            .WithArgs(new KitNameArgParser("name"))
            .WithArgs(new IntArgParser("cooldown", 0, false))
            .HandleWith(CmdSetKit);
        api.ChatCommands.Create("delkit")
            .WithDescription(Lang.Get("delkit-description"))
            .RequiresPlayer()
            .RequiresPrivilege(Privilege.root)
            .WithArgs(new KitArgParser("name"))
            .HandleWith(CmdDelKit);
        api.ChatCommands.Create("resetkit")
            .WithDescription(Lang.Get("resetkit-description"))
            .RequiresPlayer()
            .RequiresPrivilege(Privilege.root)
            .WithArgs(new KitNameArgParser("name"))
            .WithArgs(new PlayersArgParser("players", api, false))
            .HandleWith(CmdResetKit);

        api.Event.SaveGameLoaded += OnSaveGameLoaded;
        api.Event.GameWorldSave += OnGameWorldSave;
    }

    public override void Dispose() {
        base.Dispose();

        if (_api != null) {
            _api.Event.SaveGameLoaded -= OnSaveGameLoaded;
            _api.Event.GameWorldSave -= OnGameWorldSave;
        }
    }

    private void OnSaveGameLoaded() {
        KitsConfig.Load(_api!);
    }

    private void OnGameWorldSave() {
        KitsConfig.Save(_api!);
    }

    private TextCommandResult CmdKit(TextCommandCallingArgs args) {
        Kit? kit = (Kit)args[0];
        if (kit == null) {
            return TextCommandResult.Success(Lang.Get("listkit-success", string.Join(", ", KitsConfig.All())));
        }

        long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        long lastUsed = GetKitData(args.Caller.Player).GetLastUsed(kit);
        if (kit.Cooldown == 0 && lastUsed > 0) {
            return TextCommandResult.Error(Lang.Get("kit-already-used", kit.Name));
        }

        long remaining = kit.Cooldown - (now - lastUsed);
        if (remaining > 0) {
            return TextCommandResult.Error(Lang.Get("kit-on-cooldown", kit.Name, Remaining(TimeSpan.FromMilliseconds(remaining))));
        }

        IPlayerInventoryManager invManager = args.Caller.Player.InventoryManager;
        if (kit.Items.Length > invManager.GetHotbarInventory().Count(slot => slot is ItemSlotSurvival && slot.Empty)) {
            return TextCommandResult.Error(Lang.Get("kit-need-empty-slots", kit.Items.Length.ToString()));
        }

        foreach (byte[] data in kit.Items) {
            ItemStack item = new(data);
            if (item.ResolveBlockOrItem(_api!.World)) {
                invManager.TryGiveItemstack(item, true);
            } else {
                string itemInfo = $"{item.StackSize}x{item.Class.Name()[0]}{item.Id}[{item.Collectible?.Code}]";
                _mod.Logger.Error(Lang.Get(Lang.Get("kit-could-not-give-item", args.Caller.Player.PlayerName, itemInfo, kit.Name)));
            }
        }

        KitData kitData = GetKitData(args.Caller.Player);
        kitData.SetLastUsed(kit, now);
        SetKitData(args.Caller.Player, kitData);

        return TextCommandResult.Success(Lang.Get("kit-success", kit.Name));
    }

    private TextCommandResult CmdSetKit(TextCommandCallingArgs args) {
        string name = ((string)args[0]).Trim().ToLower();
        if (KitsConfig.Get(name) != null) {
            return TextCommandResult.Error(Lang.Get("kit-already-exists", name));
        }

        List<byte[]> items = new();
        foreach (ItemSlot? slot in args.Caller.Player.InventoryManager.GetHotbarInventory()) {
            if (slot.GetType() != typeof(ItemSlotSurvival) || slot.Itemstack == null) {
                continue;
            }

            ItemStack item = slot.Itemstack.Clone();
            item.Attributes.RemoveAttribute("transitionstate");
            items.Add(item.ToBytes());
        }

        // ReSharper disable once ConvertIfStatementToSwitchStatement
        if (items.Count == 0) {
            return TextCommandResult.Error(Lang.Get("kit-empty-hotbar"));
        }

        if (items.Count > 10) {
            return TextCommandResult.Error(Lang.Get("kit-hotbar-too-big"));
        }

        long cooldown = (int)args[1] * 1000;

        KitsConfig.Add(new Kit {
            Name = name,
            Items = items.ToArray(),
            Cooldown = cooldown
        });

        KitsConfig.Save(_api!);

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (cooldown == 0) {
            return TextCommandResult.Success(Lang.Get("setkit-single-use-success", name));
        }

        return TextCommandResult.Success(Lang.Get("setkit-success", name, Remaining(TimeSpan.FromMilliseconds(cooldown))));
    }

    private TextCommandResult CmdDelKit(TextCommandCallingArgs args) {
        Kit kit = (Kit)args[0];
        if (!KitsConfig.Remove(kit.Name)) {
            return TextCommandResult.Error(Lang.Get("kit-cannot-delete", kit.Name));
        }

        KitsConfig.Save(_api!);

        return TextCommandResult.Success(Lang.Get("delkit-success", kit.Name));
    }

    private TextCommandResult CmdResetKit(TextCommandCallingArgs args) {
        string? kitName = args[0]?.ToString()?.Trim().ToLower();
        if (kitName is not (null or "*")) {
            string rawName = kitName;
            kitName = KitsConfig.Get(kitName)?.Name;
            if (kitName == null) {
                return TextCommandResult.Error(Lang.Get("kit-does-not-exist", rawName));
            }
        }

        IPlayer? target = null;
        string? playerName = args[1]?.ToString()?.Trim().ToLower();
        if (playerName is not (null or "*")) {
            target = _api!.World.AllPlayers.FirstOrDefault(player => player.PlayerName.ToLower().Equals(playerName));
            playerName = target?.PlayerName;
            if (playerName == null) {
                return TextCommandResult.Error(Lang.Get("player-not-found"));
            }
        }

        if (playerName == null) {
            _api!.Event.RegisterCallback(_ => {
                if (kitName == null) {
                    SendMessage(args.Caller.Player, Lang.Success("resetkit-start-all-kits-all-players"));

                    foreach (IPlayer player in _api.World.AllPlayers) {
                        GetKitData(player).ResetAllLastUsed();
                    }

                    SendMessage(args.Caller.Player, Lang.Success("resetkit-finish-all-kits-all-players"));
                    return;
                }

                Kit? kit = KitsConfig.Get(kitName);
                if (kit == null) {
                    SendMessage(args.Caller.Player, Lang.Error("kit-does-not-exist", kitName));
                    return;
                }

                SendMessage(args.Caller.Player, Lang.Success("resetkit-start-kit-all-players", kit.Name));

                foreach (IPlayer player in _api.World.AllPlayers) {
                    GetKitData(player).ResetLastUsed(kit);
                }

                SendMessage(args.Caller.Player, Lang.Success("resetkit-finish-kit-all-players", kit.Name));
            }, 0);
            return TextCommandResult.Success();
        }

        if (target == null) {
            return TextCommandResult.Error(Lang.Get("player-not-found"));
        }

        if (kitName == null) {
            GetKitData(target).ResetAllLastUsed();
            return TextCommandResult.Success(Lang.Get("resetkit-finish-all-kits-player", target.PlayerName));
        }

        Kit? kit = KitsConfig.Get(kitName);
        if (kit == null) {
            return TextCommandResult.Error(Lang.Get("kit-does-not-exist", kitName));
        }

        GetKitData(target).ResetLastUsed(kit);
        return TextCommandResult.Success(Lang.Get("resetkit-finish-kit-player", kit.Name, target.PlayerName));
    }

    private static KitData GetKitData(IPlayer player) {
        byte[] raw = player.WorldData.GetModdata(_kitsKey);
        if (raw != null) {
            try {
                return SerializerUtil.Deserialize<KitData>(raw);
            } catch (Exception e) {
                player.Entity.World.Logger.Event(e.ToString());
            }
        }
        return new KitData();
    }

    private static void SetKitData(IPlayer player, KitData data) {
        player.WorldData.SetModdata(_kitsKey, SerializerUtil.Serialize(data));
    }

    private static void SendMessage(IPlayer player, string message) {
        (player as IServerPlayer)?.SendMessage(GlobalConstants.GeneralChatGroup, message, EnumChatType.CommandSuccess);
    }

    private static string Remaining(TimeSpan timeSpan) {
        if (timeSpan.Days > 0) {
            return Lang.Get("{p0:# days|# day|# days}", timeSpan.Days);
        }

        if (timeSpan.Hours > 0) {
            return Lang.Get("{p0:# hours|# hour|# hours}", timeSpan.Hours);
        }

        if (timeSpan.Minutes > 0) {
            return Lang.Get("{p0:# minutes|# minute|# minutes}", timeSpan.Minutes);
        }

        if (timeSpan.Seconds > 0) {
            return Lang.Get("{p0:# seconds|# second|# seconds}", timeSpan.Seconds);
        }

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (timeSpan.Milliseconds > 0) {
            return Lang.Get("{p0:# milliseconds|# millisecond|# milliseconds}", timeSpan.Milliseconds);
        }

        return timeSpan.ToString();
    }

    public class Kit {
        public required string Name { get; init; }
        public required byte[][] Items { get; init; }
        public required long Cooldown { get; init; }
    }

    private class KitsConfig {
        private static readonly KitsConfig _instance = new();

        private Dictionary<string, Kit> _kits = new();

        public static void Add(Kit kit) {
            _instance._kits.Add(kit.Name, kit);
        }

        public static Kit? Get(string name) {
            return _instance._kits!.Get(name);
        }

        public static bool Remove(string name) {
            return _instance._kits.Remove(name);
        }

        public static string[] All() {
            return _instance._kits.Keys.ToArray();
        }

        public static void Load(ICoreServerAPI api) {
            byte[]? data = api.WorldManager.SaveGame.GetData(_kitsKey);
            try {
                _instance._kits = data == null
                    ? new Dictionary<string, Kit>()
                    : SerializerUtil.Deserialize<Dictionary<string, Kit>>(data);
            } catch (Exception) {
                _instance._kits = new Dictionary<string, Kit>();
            }
        }

        public static void Save(ICoreServerAPI api) {
            byte[] data = SerializerUtil.Serialize(_instance._kits);
            api.WorldManager.SaveGame.StoreData(_kitsKey, data);
        }
    }

    public class KitData {
        private readonly Dictionary<string, long> _lastUsed = new();

        public long GetLastUsed(Kit kit) {
            return _lastUsed.GetValueOrDefault(kit.Name);
        }

        public void SetLastUsed(Kit kit, long millis) {
            _lastUsed[kit.Name] = millis;
        }

        public void ResetLastUsed(Kit kit) {
            _lastUsed.Remove(kit.Name);
        }

        public void ResetAllLastUsed() {
            _lastUsed.Clear();
        }
    }

    private class KitArgParser : ArgumentParserBase {
        private Kit? _kit;

        public KitArgParser(string argName) : base(argName, true) { }

        public override Kit? GetValue() => _kit;

        public override void SetValue(object data) => _kit = (Kit)data;

        public override void PreProcess(TextCommandCallingArgs args) {
            base.PreProcess(args);
            _kit = null;
        }

        public override EnumParseResult TryProcess(TextCommandCallingArgs args, Action<AsyncParseResults>? onReady = null) {
            string? arg = args.RawArgs.PopWord()?.ToLower();
            if (arg == null) {
                lastErrorMessage = Lang.Get("must-specify-kit");
                return EnumParseResult.Bad;
            }

            _kit = KitsConfig.Get(arg);
            if (_kit != null) {
                return EnumParseResult.Good;
            }

            lastErrorMessage = Lang.Get("kit-does-not-exist", arg);
            return EnumParseResult.Bad;
        }
    }

    private class KitNameArgParser : ArgumentParserBase {
        private string? _name;

        public KitNameArgParser(string argName) : base(argName, true) { }

        public override string? GetValue() => _name;

        public override void SetValue(object data) => _name = (string)data;

        public override void PreProcess(TextCommandCallingArgs args) {
            base.PreProcess(args);
            _name = null;
        }

        public override EnumParseResult TryProcess(TextCommandCallingArgs args, Action<AsyncParseResults>? onReady = null) {
            string? arg = args.RawArgs.PopWord()?.ToLower();
            if (arg == null) {
                lastErrorMessage = Lang.Get("must-specify-kit");
                return EnumParseResult.Bad;
            }

            _name = arg;
            if (!ValidNameRegex().IsMatch(_name)) {
                lastErrorMessage = Lang.Get("invalid-kit-name");
                return EnumParseResult.Bad;
            }

            return EnumParseResult.Good;
        }
    }
}
