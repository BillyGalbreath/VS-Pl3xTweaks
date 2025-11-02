using pl3xtweaks.util;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace pl3xtweaks.module;

public class BlockParticles(Pl3xTweaks __mod) : Module(__mod) {
    private Pl3xParticles[] _particles = [];

    private bool Enabled {
        get => _mod.ClientData.GetData<bool>("block-particles", true);
        set => _mod.ClientData.SaveData("block-particles", value);
    }

    public override void AssetsFinalize(ICoreAPI api) {
        if (api is not ICoreClientAPI) {
            return;
        }

        _particles = [
            new SoilParticles(),
            new MeteoriteParticles(),
            new CursedParticles(),
            new ResinParticles()
        ];

        SetEnabledState(api, Enabled);
    }

    public override void StartClientSide(ICoreClientAPI api) {
        api.Input.RegisterHotKey("block-particles-toggle", Lang.Get("block-particles-toggle"), GlKeys.F8, HotkeyType.GUIOrOtherControls);
        api.Input.SetHotKeyHandler("block-particles-toggle", _ => {
            SetEnabledState(api, !Enabled);
            return true;
        });
    }

    private void SetEnabledState(ICoreAPI api, bool enabled) {
        // ReSharper disable once AssignmentInConditionalExpression
        if (Enabled = enabled) {
            _particles.Foreach(particles => particles.Enable(api));
            (api as ICoreClientAPI)?.ShowChatMessage(Lang.Get("block-particles-enabled"));
        } else {
            _particles.Foreach(particles => particles.Disable(api));
            (api as ICoreClientAPI)?.ShowChatMessage(Lang.Get("block-particles-disabled"));
        }
    }

    private abstract class Pl3xParticles {
        private AssetLocation[] Blocks { get; }

        protected AdvancedParticleProperties[]? _properties;
        protected abstract AdvancedParticleProperties[] ParticleProperties { get; }

        private readonly Dictionary<string, AdvancedParticleProperties[]> _originalProperties = new();

        protected Pl3xParticles(params string[] blocks) {
            Blocks = blocks.Select(block => new AssetLocation(block)).ToArray();
        }

        public void Enable(ICoreAPI capi) {
            Blocks.Foreach(matchBlocks => {
                capi.World.SearchBlocks(matchBlocks).Foreach(block => {
                    _originalProperties[block.Code] = block.ParticleProperties;
                    block.ParticleProperties ??= [];
                    block.ParticleProperties = block.ParticleProperties.Append(ParticleProperties);
                });
            });
        }

        public void Disable(ICoreAPI capi) {
            Blocks.Foreach(matchBlocks => {
                capi.World.SearchBlocks(matchBlocks).Foreach(block => {
                    block.ParticleProperties = _originalProperties[block.Code];
                });
            });
        }

        protected static NatFloat NatF(float avg, float var) {
            return NatFloat.createUniform(avg, var);
        }

        private static NatFloat NatF(float[] floats) {
            return floats.Length switch {
                1 => NatF(floats[0], floats[0]),
                2 => NatF(floats[0], floats[1]),
                _ => throw new ArgumentOutOfRangeException(nameof(floats), "The length of array must be 1 or 2")
            };
        }

        protected static NatFloat[] NatF(params float[][] floats) {
            return floats.Length switch {
                3 => [NatF(floats[0]), NatF(floats[1]), NatF(floats[2])],
                4 => [NatF(floats[0]), NatF(floats[1]), NatF(floats[2]), NatF(floats[3])],
                _ => throw new ArgumentOutOfRangeException(nameof(floats), "The length of array must be 3 or 4")
            };
        }
    }

    private class SoilParticles() : Pl3xParticles("soil-high-*", "soil-compost-*", "peat-*", "rawclay-blue-*", "rawclay-red-*") {
        protected override AdvancedParticleProperties[] ParticleProperties => _properties ??= [
            new AdvancedParticleProperties {
                HsvaColor = NatF([17, 0], [255, 0], [36, 0], [200, 50]),
                PosOffset = NatF([0, 0.5f], [-0.5f, 0], [0, 0.5f]),
                Velocity = NatF([0, 0], [0, 0.2f], [0, 0.5f]),
                Quantity = NatF(0.1f, 0.05f),
                LifeLength = NatF(1, 0),
                GravityEffect = NatF(-0.06f, 0.01f),
                Size = NatF(0.4f, 0.1f),
                SizeEvolve = new EvolvingNatFloat(EnumTransformFunction.QUADRATIC, 1.2f),
                OpacityEvolve = new EvolvingNatFloat(EnumTransformFunction.QUADRATIC, -16),
                ParticleModel = EnumParticleModel.Quad,
                WindAffectednes = 1
            }
        ];
    }

    private class MeteoriteParticles() : Pl3xParticles("meteorite-iron") {
        protected override AdvancedParticleProperties[] ParticleProperties => _properties ??= [
            new AdvancedParticleProperties {
                HsvaColor = NatF([24, 15], [177, 0], [255, 0], [255, 100]),
                PosOffset = NatF([0, 0.5f], [0, 2], [0, 0.5f]),
                Velocity = NatF([0, 0.5f], [10, 5], [0, 0.5f]),
                Quantity = NatF(1, 1),
                LifeLength = NatF(0.25f, 1),
                GravityEffect = NatF(0, 0),
                Size = NatF(0.5f, 0.2f),
                OpacityEvolve = new EvolvingNatFloat(EnumTransformFunction.QUADRATIC, -30),
                ParticleModel = EnumParticleModel.Quad,
                VertexFlags = 128
            }
        ];
    }

    private class CursedParticles : Pl3xParticles {
        public CursedParticles() : base("lootvessel-*", "bonysoil") {
            AdvancedParticleProperties[] p = new AdvancedParticleProperties[2];
            Array.Fill(p, new AdvancedParticleProperties {
                HsvaColor = NatF([128, 0], [0, 0], [0, 0], [255, 0]),
                PosOffset = NatF([0, 0.4f], [0, 0.0f], [0, 0.4f]),
                Velocity = NatF([0, 0.5f], [0, 0.5f], [0, 0.5f]),
                Quantity = NatF(5, 0),
                LifeLength = NatF(0.3f, 0),
                GravityEffect = NatF(-0.4f, 0),
                Size = NatF(0.3f, 0.1f)
            });

            p[1].HsvaColor = NatF([36, 0], [255, 0], [255, 0], [255, 0]);
            p[1].PosOffset = NatF([0, 0.4f], [0, 0.9f], [0, 0.4f]);

            _properties = [p[0], p[1]];
        }

        protected override AdvancedParticleProperties[] ParticleProperties => _properties!;
    }

    private class ResinParticles : Pl3xParticles {
        public ResinParticles() : base("log-resin-*") {
            AdvancedParticleProperties[] p = new AdvancedParticleProperties[4];
            Array.Fill(p, new AdvancedParticleProperties {
                HsvaColor = NatF([22, 3], [236, 0], [252, 0], [199, 50]),
                PosOffset = NatF([0, 0.2f], [-0.5f, 0], [0.6f, 0]),
                Velocity = NatF([0, 0.5f], [0, 0.5f], [0, 0.5f]),
                Quantity = NatF(0.05f, 0),
                LifeLength = NatF(1.2f, 0),
                GravityEffect = NatF(0.5f, 0),
                Size = NatF(1.2f, 0.2f),
                SizeEvolve = EvolvingNatFloat.createIdentical(1.0f),
                VertexFlags = 8192,
                WindAffectednes = 1
            });

            p[1].PosOffset = NatF([0, 0.2f], [-0.5f, 0], [-0.6f, 0]);
            p[2].PosOffset = NatF([-0.6f, 0], [-0.5f, 0], [0, 0.2f]);
            p[3].PosOffset = NatF([0.6f, 0], [-0.5f, 0], [0, 0.2f]);

            _properties = [p[0], p[1], p[2], p[3]];
        }

        protected override AdvancedParticleProperties[] ParticleProperties => _properties!;
    }
}
