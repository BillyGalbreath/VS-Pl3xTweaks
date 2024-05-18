using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using YamlDotNet.Serialization;

namespace Pl3xTweaks.configuration;

[SuppressMessage("ReSharper", "CollectionNeverUpdated.Global")]
[SuppressMessage("ReSharper", "ConvertToConstant.Global")]
[SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class TipsConfig {
    [YamlMember(Order = 0, Description = null)]
    public int Interval = 3600000; // 1 hour in millis

    [YamlMember(Order = 1, Description = null)]
    public string Prefix = "[Tip]: {0}";

    [YamlMember(Order = 2, Description = null)]
    public List<string> List = new();

    private const string DisabledKey = "Pl3xTweaks:DisableTips";
    private static readonly List<string> CachedList = new();
    private static long _lastTipTime;

    public static void Reset() {
        CachedList.RemoveAll(_ => true);
    }

    public static void Tick(ICoreServerAPI api) {
        long now = api.World.ElapsedMilliseconds;
        if (_lastTipTime > 0 && now - _lastTipTime < Config.TipsConfig.Interval) {
            return;
        }

        _lastTipTime = now;

        int count = CachedList.Count;
        if (count <= 0) {
            CachedList.AddRange(Config.TipsConfig.List);
            _lastTipTime = 0;
            return;
        }

        int index = api.World.Rand.Next(count);
        string tip = string.Format(Config.TipsConfig.Prefix, CachedList[index]);
        CachedList.RemoveAt(index);

        List<IServerPlayer>? players = ((IServerPlayer[])api.World.AllOnlinePlayers)?.ToList();
        if (players == null) {
            return;
        }

        foreach (IServerPlayer player in players.Where(player => player.ConnectionState == EnumClientState.Playing && !player.GetModData<bool>(DisabledKey))) {
            player.SendMessage(GlobalConstants.GeneralChatGroup, tip, EnumChatType.Notification);
        }
    }

    public static TextCommandResult Execute(TextCommandCallingArgs args) {
        bool enabled = (bool)args[0];
        ((IServerPlayer)args.Caller.Player).SetModData(DisabledKey, !enabled);
        return TextCommandResult.Success($"Tips are now {(enabled ? "on" : "off")}.");
    }
}
