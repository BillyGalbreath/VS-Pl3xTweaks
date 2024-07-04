using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace pl3xtweaks.module;

public class BlockParticles : Module {
    private readonly string[] _blocks = { "soil-high-*", "soil-compost-*" };
    private readonly AdvancedParticleProperties[] _particles = { new CustomParticleProperties() };

    private bool _enabled = true; // todo - save preference

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
        api.Input.RegisterHotKey("pl3xtweaks:block-particles-toggle", Lang.Get("pl3xtweaks:block-particles-toggle"), GlKeys.F8, HotkeyType.GUIOrOtherControls);
        api.Input.SetHotKeyHandler("pl3xtweaks:block-particles-toggle", Toggle);
    }

    private bool Toggle(KeyCombination combo) {
        if (_enabled) {
            Disable();
        } else {
            Enable();
        }
        return true;
    }

    private void Enable() {
        _enabled = true;
        _mod.Api.ShowChatMessage(Lang.Get("pl3xtweaks:block-particles-enabled"));
        foreach (Block block in _mod.Api.World.Blocks) {
            if (block.WildCardMatch(_blocks)) {
                block.ParticleProperties.Foreach(particles => {
                    if (particles is CustomParticleProperties custom) {
                        custom.Enable();
                    }
                });
            }
        }
    }

    private void Disable() {
        _enabled = false;
        _mod.Api.ShowChatMessage(Lang.Get("pl3xtweaks:block-particles-disabled"));
        foreach (Block block in _mod.Api.World.Blocks) {
            if (block.WildCardMatch(_blocks)) {
                block.ParticleProperties.Foreach(particles => {
                    if (particles is CustomParticleProperties custom) {
                        custom.Disable();
                    }
                });
            }
        }
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
