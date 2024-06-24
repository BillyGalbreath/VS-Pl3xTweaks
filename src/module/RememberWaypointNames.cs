using System.Diagnostics.CodeAnalysis;
using Vintagestory.API.Client;
using Vintagestory.GameContent;

namespace pl3xtweaks.module;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class RememberWaypointNames : Module {
    private static WaypointNames _waypointNames = null!;

    public RememberWaypointNames(Pl3xTweaks mod) : base(mod) {
        _waypointNames = new WaypointNames(mod);

        mod.Patch<GuiDialogAddWayPoint>("autoSuggestName", prefix: Prefix);
        mod.Patch<GuiDialogAddWayPoint>("onSave", postfix: Postfix);
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

        public WaypointNames(Pl3xTweaks mod) {
            _mod = mod;
        }

        public string? Get(string index) {
            return Read().GetValueOrDefault(index);
        }

        public void Set(string index, string name) {
            Dictionary<string, string?> names = Read();
            names[index] = name;
            _mod.Api.StoreModConfig(names, $"{_mod.ModId}.json");
        }

        private Dictionary<string, string?> Read() {
            return _mod.Api.LoadModConfig<Dictionary<string, string?>>($"{_mod.ModId}.json") ?? new Dictionary<string, string?>();
        }
    }
}
