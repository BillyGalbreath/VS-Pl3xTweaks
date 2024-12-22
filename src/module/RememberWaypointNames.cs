using Newtonsoft.Json.Linq;
using Vintagestory.API.Client;
using Vintagestory.GameContent;

namespace pl3xtweaks.module;

public class RememberWaypointNames : Module {
    private static WaypointNames _waypointNames = null!;

    public RememberWaypointNames(Pl3xTweaks mod) : base(mod) { }

    public override void StartClientSide(ICoreClientAPI api) {
        _waypointNames = new WaypointNames(_mod);
        _mod.Patch<GuiDialogAddWayPoint>("autoSuggestName", prefix: Prefix);
        _mod.Patch<GuiDialogAddWayPoint>("onSave", postfix: Postfix);
    }

    private static bool Prefix(GuiDialogAddWayPoint __instance, string ___curIcon, string ___curColor, ref bool ___ignoreNextAutosuggestDisable) {
        string? savedName = _waypointNames.Get($"{___curIcon}-{___curColor}");
        if (string.IsNullOrEmpty(savedName)) {
            return true;
        }

        GuiElementTextInput textInput = __instance.SingleComposer.GetTextInput("nameInput");
        ___ignoreNextAutosuggestDisable = true;
        textInput.SetValue(savedName);
        return false;
    }

    private static void Postfix(GuiDialogAddWayPoint __instance, ref string ___curIcon, ref string ___curColor) {
        string curName = __instance.SingleComposer.GetTextInput("nameInput").GetText();
        _waypointNames.Set($"{___curIcon}-{___curColor}", curName);
    }

    public override void Dispose() {
        base.Dispose();
        _waypointNames = null!;
    }

    private class WaypointNames {
        private readonly Pl3xTweaks _mod;

        internal WaypointNames(Pl3xTweaks mod) {
            _mod = mod;
        }

        internal string? Get(string index) {
            return Get().GetValue(index)?.Value<string>();
        }

        internal void Set(string index, string name) {
            JObject waypoints = Get();
            waypoints[index] = name;
            _mod.ClientData.SaveData("waypoints", waypoints);
        }

        private JObject Get() {
            return _mod.ClientData.GetData<JObject>("waypoints") ?? new JObject();
        }
    }
}
