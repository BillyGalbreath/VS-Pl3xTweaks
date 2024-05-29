using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using pl3xtweaks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace Pl3xTweaks.module;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class RememberWaypointNames : Module {
    private static readonly WaypointNames _waypointNames = new(TweaksMod.Api!);

    public RememberWaypointNames(TweaksMod mod) {
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

    private class WaypointNames {
        private readonly ICoreAPI _api;

        public WaypointNames(ICoreAPI api) {
            _api = api;
        }

        public string? Get(string index) {
            return Read().GetValueOrDefault(index);
        }

        public void Set(string index, string name) {
            Dictionary<string, string?> names = Read();
            names[index] = name;
            _api.StoreModConfig(names, $"{TweaksMod.Id}.json");
        }

        private Dictionary<string, string?> Read() {
            return _api.LoadModConfig<Dictionary<string, string?>>($"{TweaksMod.Id}.json") ?? new Dictionary<string, string?>();
        }
    }
}
