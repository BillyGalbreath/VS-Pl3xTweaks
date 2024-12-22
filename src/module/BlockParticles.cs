using pl3xtweaks.util;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace pl3xtweaks.module;

public class BlockParticles : Module {
    private readonly string[] _blocks = { "soil-high-*", "soil-compost-*" };
    private readonly CustomParticleProperties _particles = new();

    private ICoreClientAPI _capi = null!;
    private bool _enabled;

    public BlockParticles(Pl3xTweaks mod) : base(mod) { }

    public override void AssetsFinalize(ICoreAPI api) {
        foreach (Block block in api.World.Blocks) {
            if (block.WildCardMatch(_blocks)) {
                block.ParticleProperties ??= Array.Empty<AdvancedParticleProperties>();
                block.ParticleProperties = block.ParticleProperties.Append(_particles);
            }
        }
    }

    public override void StartClientSide(ICoreClientAPI api) {
        _capi = api;

        _capi.Input.RegisterHotKey("block-particles-toggle", Lang.Get("block-particles-toggle"), GlKeys.F8, HotkeyType.GUIOrOtherControls);
        _capi.Input.SetHotKeyHandler("block-particles-toggle", _ => Set(!_enabled));

        Set(_mod.ClientData.GetData<bool>("block-particles"));
    }

    private bool Set(bool enabled) {
        if (enabled) {
            _capi.ShowChatMessage(Lang.Get("block-particles-enabled"));
            _particles.Enable();
            _mod.ClientData.SaveData("block-particles", _enabled = true);
        } else {
            _capi.ShowChatMessage(Lang.Get("block-particles-disabled"));
            _particles.Disable();
            _mod.ClientData.SaveData("block-particles", _enabled = false);
        }
        return true;
    }

    private class CustomParticleProperties : AdvancedParticleProperties {
        public CustomParticleProperties() {
            HsvaColor = new[] {
                NatFloat.createUniform(0, 0),
                NatFloat.createUniform(100, 0),
                NatFloat.createUniform(40, 10),
                NatFloat.createUniform(0, 0)
            };
            GravityEffect = NatFloat.createUniform(-0.005f, 0);
            PosOffset = new[] {
                NatFloat.createUniform(0, 0.5f),
                NatFloat.createUniform(0, 0.5f),
                NatFloat.createUniform(0, 0.5f)
            };
            Velocity = new[] {
                NatFloat.createUniform(0, 0.05f),
                NatFloat.createUniform(0.5f, 0.05f),
                NatFloat.createUniform(0, 0.05f)
            };
            WindAffectednes = 0;
            Quantity = NatFloat.createUniform(0, 1.5f);
            TerrainCollision = false;
            Size = NatFloat.createUniform(0.5f, 0.05f);
            SizeEvolve = new EvolvingNatFloat(EnumTransformFunction.LINEAR, 0.5f);
            ParticleModel = EnumParticleModel.Quad;
            LifeLength = NatFloat.createUniform(1, 0.25f);
        }

        public void Enable() {
            HsvaColor[3] = NatFloat.createUniform(255, 0);
        }

        public void Disable() {
            HsvaColor[3] = NatFloat.createUniform(0, 0);
        }
    }
}
