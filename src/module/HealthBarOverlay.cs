using Vintagestory.API.Client;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace pl3xtweaks.module;

public class HealthBarOverlay : Module {
    private ICoreClientAPI? _api;

    public HealthBarOverlay(Pl3xTweaks mod) : base(mod) { }

    public override void StartClientSide(ICoreClientAPI api) {
        _api = _mod.Api;
        api.RegisterEntityBehaviorClass("healthbaroverlay", typeof(HealthBarOverlayBehavior));
        api.Event.PlayerEntitySpawn += OnPlayerEntitySpawn;
    }

    private static void OnPlayerEntitySpawn(IClientPlayer player) {
        player.Entity.AddBehavior(new HealthBarOverlayBehavior(player.Entity));
    }

    public override void Dispose() {
        if (_api != null) {
            _api.Event.PlayerEntitySpawn -= OnPlayerEntitySpawn;
        }
    }

    private class HealthBarOverlayBehavior : EntityBehavior {
        private readonly HealthBarRenderer _renderer;

        public HealthBarOverlayBehavior(Entity entity) : base(entity) {
            _renderer = new HealthBarRenderer((ICoreClientAPI)entity.Api);
        }

        public override string PropertyName() => "healthbaroverlay";

        public override void OnGameTick(float dt) {
            _renderer.OnGameTick();
        }

        public override void OnEntityDespawn(EntityDespawnData despawn) {
            _renderer.Dispose();
        }
    }

    private class HealthBarRenderer : IRenderer {
        private readonly ICoreClientAPI _api;

        private readonly Matrixf _mvMatrix = new();
        private readonly MeshRef? _healthBarRef;
        private readonly MeshRef? _backRef;

        private readonly Vec4f _color = new();
        private float _alpha;

        private float _healthPercent;

        private Entity? _entity;
        private bool _active;

        public double RenderOrder => 0.41; // After Entity 0.4
        public int RenderRange => 10;

        public HealthBarRenderer(ICoreClientAPI api) {
            _api = api;

            _backRef = _api.Render.UploadMesh(LineMeshUtil.GetRectangle());
            _healthBarRef = _api.Render.UploadMesh(QuadMeshUtil.GetQuad());

            _api.Event.RegisterRenderer(this, EnumRenderStage.Ortho);
        }

        public void OnGameTick() {
            Entity? target = _api.World.Player.CurrentEntitySelection?.Entity;
            if (target == null) {
                _active = false;
            } else {
                _entity = target;
                _active = true;
            }

            ITreeAttribute? tree = _entity?.WatchedAttributes.GetTreeAttribute("health");
            if (tree == null) {
                return;
            }

            _healthPercent = tree.GetFloat("currenthealth") / tree.GetFloat("maxhealth");

            int color = _healthPercent switch {
                <= 0.25f => 0xBF7F7F,
                <= 0.5f => 0xBFBF7F,
                _ => 0x7FBF7F
            };
            _color.Set(
                (color >> 16 & 0xFF) / 255f,
                (color >> 8 & 0xFF) / 255f,
                (color & 0xFF) / 255f,
                _alpha
            );
        }

        public void OnRenderFrame(float deltaTime, EnumRenderStage stage) {
            if (_entity == null || (_alpha <= 0 && !_active)) {
                return;
            }

            _color.A = _alpha = float.Clamp(_alpha + (deltaTime / (_active ? 0.2f : -0.4f)), 0, 1);

            Vec3d pos = MatrixToolsd.Project(
                new Vec3d(_entity.Pos.X, _entity.Pos.Y, _entity.Pos.Z).Add(
                    _entity.CollisionBox.X2 - _entity.OriginCollisionBox.X2,
                    _entity.CollisionBox.MaxY,
                    _entity.CollisionBox.Z2 - _entity.OriginCollisionBox.Z2
                ),
                _api.Render.PerspectiveProjectionMat,
                _api.Render.PerspectiveViewMat,
                _api.Render.FrameWidth,
                _api.Render.FrameHeight);

            // Z negative seems to indicate that the name tag is behind us \o/
            if (pos.Z < 0) {
                return;
            }

            float scale = Math.Min(1f, 4f / Math.Max(1, (float)pos.Z));
            if (scale > 0.75f) {
                scale = 0.75f + (scale - 0.75f) / 2;
            }

            float x = (float)pos.X - scale * 100 / 2;
            float y = _api.Render.FrameHeight - (float)pos.Y - (10 * Math.Max(0, scale)) - 10;
            float w = scale * 100;
            float h = scale * 10;

            IShaderProgram shader = _api.Render.CurrentActiveShader;
            shader.Uniform("rgbaIn", _color);
            shader.Uniform("extraGlow", 0);
            shader.Uniform("applyColor", 0);
            shader.Uniform("tex2d", 0);
            shader.Uniform("noTexture", 1f);

            // Render back
            _mvMatrix.Set(_api.Render.CurrentModelviewMatrix).Translate(x, y, 20).Scale(w, h, 0).Translate(0.5f, 0.5f, 0).Scale(0.5f, 0.5f, 0);
            shader.UniformMatrix("projectionMatrix", _api.Render.CurrentProjectionMatrix);
            shader.UniformMatrix("modelViewMatrix", _mvMatrix.Values);
            _api.Render.RenderMesh(_backRef);

            // Render health bar
            _mvMatrix.Set(_api.Render.CurrentModelviewMatrix).Translate(x, y, 20).Scale(w * _healthPercent, h, 0).Translate(0.5f, 0.5f, 0).Scale(0.5f, 0.5f, 0);
            shader.UniformMatrix("projectionMatrix", _api.Render.CurrentProjectionMatrix);
            shader.UniformMatrix("modelViewMatrix", _mvMatrix.Values);
            _api.Render.RenderMesh(_healthBarRef);
        }

        public void Dispose() {
            _api.Render.DeleteMesh(_backRef);
            _api.Render.DeleteMesh(_healthBarRef);
            _api.Event.UnregisterRenderer(this, EnumRenderStage.Ortho);
        }
    }
}
