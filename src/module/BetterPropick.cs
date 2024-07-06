using System.Diagnostics.CodeAnalysis;
using pl3xtweaks.util;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;
using Lang = pl3xtweaks.util.Lang;

namespace pl3xtweaks.module;

public class BetterPropick : Module {
    private static readonly SkillItem _coreMode = new() {
        Code = new AssetLocation("core"),
        Name = Lang.Get("Core Sample Mode (Searches in a straight line)")
    };

    public BetterPropick(Pl3xTweaks mod) : base(mod) { }

    public override void Start(ICoreAPI api) {
        _mod.Patch<ItemProspectingPick>("OnBlockBrokenWith", Prefix);
        _mod.Patch<ItemProspectingPick>("OnLoaded", postfix: Postfix);
    }

    public override void StartClientSide(ICoreClientAPI api) {
        _coreMode.WithIcon(api, api.Gui.LoadSvgWithPadding(new AssetLocation("pl3xtweaks:textures/icons/coresample.svg"), 48, 48, 5, ColorUtil.WhiteArgb));
        _coreMode.TexturePremultipliedAlpha = false;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private static bool Prefix(ItemProspectingPick __instance, ref bool __result, IWorldAccessor world, Entity byEntity, ItemSlot itemslot, BlockSelection blockSel, SkillItem[] ___toolModes, ICoreAPI ___api) {
        if (__instance.GetToolMode(itemslot, (byEntity as EntityPlayer)?.Player, blockSel) == 1) {
            ProbeCoreSampleMode(__instance, ___api, world, byEntity, blockSel);
            __result = true;
            return false;
        }

        return true;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private static void Postfix(ICoreAPI api, ref SkillItem[] ___toolModes) {
        List<SkillItem> list = ___toolModes.ToList();
        list.Insert(1, _coreMode);
        ___toolModes = list.ToArray();
    }

    private static void ProbeCoreSampleMode(ItemProspectingPick instance, ICoreAPI api, IWorldAccessor world, Entity byEntity, BlockSelection blockSel) {
        IPlayer? byPlayer = byEntity is EntityPlayer ePlayer ? world.PlayerByUid(ePlayer.PlayerUID) : null;
        Block block = world.BlockAccessor.GetBlock(blockSel.Position);
        block.OnBlockBroken(world, blockSel.Position, byPlayer, 0.0f);
        if (!instance.Invoke<bool>("isPropickable", new object?[] { block }) || byPlayer is not IServerPlayer sPlayer) {
            return;
        }

        sPlayer.SendMessage(GlobalConstants.InfoLogChatGroup, Lang.GetL(sPlayer.LanguageCode, "Core sample taken for depth 64:"), EnumChatType.Notification);

        Dictionary<string, int> quantityFound = new();
        BlockPos pos = blockSel.Position.Copy();
        for (int i = 0; i < 64; i++) {
            Block nblock = api.World.BlockAccessor.GetBlock(pos);
            if (nblock.BlockMaterial == EnumBlockMaterial.Ore && nblock.Variant.TryGetValue("type", out string? value)) {
                string key = "ore-" + value;
                quantityFound.TryGetValue(key, out int count);
                quantityFound[key] = count + 1;
            }
            pos.Add(blockSel.Face, -1);
        }

        if (quantityFound.Count == 0) {
            sPlayer.SendMessage(GlobalConstants.InfoLogChatGroup, Lang.GetL(sPlayer.LanguageCode, "No ore node found"), EnumChatType.Notification);
            return;
        }

        sPlayer.SendMessage(GlobalConstants.InfoLogChatGroup, Lang.GetL(sPlayer.LanguageCode, "Found the following ore nodes"), EnumChatType.Notification);

        List<KeyValuePair<string, int>> ordered = quantityFound.OrderByDescending(val => val.Value).ToList();
        foreach ((string? key, int value) in ordered) {
            string orename = Lang.GetL(sPlayer.LanguageCode, key);
            string resultText = Lang.GetL(sPlayer.LanguageCode, instance.Invoke<string>("resultTextByQuantity", new object?[] { value })!, Lang.Get(key));
            sPlayer.SendMessage(GlobalConstants.InfoLogChatGroup, Lang.GetL(sPlayer.LanguageCode, resultText, orename), EnumChatType.Notification);
        }
    }
}
