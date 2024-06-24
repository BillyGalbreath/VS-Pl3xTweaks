using System.Diagnostics.CodeAnalysis;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace pl3xtweaks.module;

public class BetterPropick : Module {
    private static readonly AssetLocation _code = new("core");

    public BetterPropick(Pl3xTweaks mod) : base(mod) {
        mod.Patch<ItemProspectingPick>("OnBlockBrokenWith", Prefix);
        mod.Patch<ItemProspectingPick>("OnLoaded", postfix: Postfix);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private static bool Prefix(ItemProspectingPick __instance, ref bool __result, IWorldAccessor world, Entity byEntity, ItemSlot itemslot, BlockSelection blockSel, SkillItem[] ___toolModes) {
        int toolMode = __instance.GetToolMode(itemslot, (byEntity as EntityPlayer)?.Player, blockSel);
        if (toolMode == 1) {
            ProbeCoreSampleMode(world, byEntity, blockSel);
            __result = true;
            return false;
        }

        return true;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private static void Postfix(ICoreAPI api, ref SkillItem[] ___toolModes) {
        SkillItem coreMode = new() {
            Code = _code,
            Name = Lang.Get("Core Sample Mode (Searches in a straight line)")
        };
        if (api is ICoreClientAPI capi) {
            coreMode.WithIcon(capi, capi.Gui.LoadSvgWithPadding(new AssetLocation("pl3xtweaks:textures/icons/coresample.svg"), 48, 48, 5, ColorUtil.WhiteArgb));
            coreMode.TexturePremultipliedAlpha = false;
        }

        List<SkillItem> list = ___toolModes.ToList();
        list.Insert(1, coreMode);
        ___toolModes = list.ToArray();
    }

    private static void ProbeCoreSampleMode(IWorldAccessor world, Entity byEntity, BlockSelection blockSel) {
        IPlayer? byPlayer = byEntity is EntityPlayer ePlayer ? world.PlayerByUid(ePlayer.PlayerUID) : null;
        Block block = world.BlockAccessor.GetBlock(blockSel.Position);
        block.OnBlockBroken(world, blockSel.Position, byPlayer, 0.0f);
    }
}
