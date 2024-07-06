using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace pl3xtweaks.module;

public class Tips : Module {
    private const string _disabledKey = "servertweaks:disabletips";

    private readonly List<string> _cachedList = new();

    private ICoreServerAPI? _api;
    private long _tickId;
    private long _lastTipTime;

    public Tips(Pl3xTweaks mod) : base(mod) { }

    public override void StartServerSide(ICoreServerAPI api) {
        _api = api;
        _tickId = api.Event.RegisterGameTickListener(Tick, 1000);

        api.ChatCommands.Create("tips")
            .WithDescription("Toggles server tips on and off")
            .RequiresPlayer()
            .RequiresPrivilege(Privilege.chat)
            .WithArgs(new BoolArgParser("enabled", "true", true))
            .HandleWith(Execute);
    }

    private void Tick(float _) {
        long now = _api!.World.ElapsedMilliseconds;
        if (_lastTipTime > 0 && now - _lastTipTime < _mod.Config.Tips.Interval) {
            return;
        }

        _lastTipTime = now;

        int count = _cachedList.Count;
        if (count == 0) {
            _cachedList.AddRange(_mod.Config.Tips.List);
            _lastTipTime = 0;
            return;
        }

        int index = _api.World.Rand.Next(count);
        string tip = string.Format(_mod.Config.Tips.Prefix, _cachedList[index]);
        _cachedList.RemoveAt(index);

        foreach (IServerPlayer player in _api.World.AllOnlinePlayers.Cast<IServerPlayer>()) {
            if (player.ConnectionState != EnumClientState.Playing) {
                return;
            }
            if (player.GetModData<bool>(_disabledKey)) {
                return;
            }
            player.SendMessage(GlobalConstants.GeneralChatGroup, tip, EnumChatType.Notification);
        }
    }

    private static TextCommandResult Execute(TextCommandCallingArgs args) {
        bool enabled = (bool)args[0];
        ((IServerPlayer)args.Caller.Player).SetModData(_disabledKey, !enabled);
        return TextCommandResult.Success($"Tips are now {(enabled ? "on" : "off")}.");
    }

    public override void Reload() {
        _cachedList.Clear();
    }

    public override void Dispose() {
        _api?.Event.UnregisterGameTickListener(_tickId);
        _cachedList.Clear();
    }
}
