using Vintagestory.API.Client;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace pl3xtweaks.module;

public class HealthBarOverlay : Module {
    private ICoreClientAPI? _api;

    private HealthBarRenderer? _healthBarRenderer;

    private long _tickId;

    public HealthBarOverlay(Pl3xTweaks mod) : base(mod) { }

    public override void StartClientSide(ICoreClientAPI api) {
        _api = api;

        _healthBarRenderer = new HealthBarRenderer(_api);

        _tickId = _api.Event.RegisterGameTickListener(OnGameTick, 50, 50);
    }

    private void OnGameTick(float deltaTime) {
        _healthBarRenderer?.OnGameTick();
    }

    public override void Dispose() {
        _api?.Event.UnregisterGameTickListener(_tickId);

        _healthBarRenderer?.Dispose();
    }

    private class HealthBarRenderer : IRenderer {
        private readonly ICoreClientAPI _api;

        private readonly Matrixf _mvMatrix = new();
        private readonly MeshRef? _fgMesh;
        private readonly MeshRef? _bgMesh;

        private readonly Vec4f _color = new();

        private float _progress;
        private float _alpha;

        private Entity? _entity;
        private bool _active;

        public double RenderOrder => 0.41; // After Entity 0.4
        public int RenderRange => 10;

        public HealthBarRenderer(ICoreClientAPI api) {
            _api = api;

            _bgMesh = _api.Render.UploadMesh(LineMeshUtil.GetRectangle());
            _fgMesh = _api.Render.UploadMesh(QuadMeshUtil.GetQuad());

            _api.Event.RegisterRenderer(this, EnumRenderStage.Ortho);
        }

        public void OnGameTick() {
            Entity? target = _api.World.Player.CurrentEntitySelection?.Entity;
            if (target is { IsCreature: true } and not EntityBoat) {
                _entity = target;
                _active = true;
            } else {
                _active = false;
            }

            ITreeAttribute? tree = _entity?.WatchedAttributes.GetTreeAttribute("health");
            if (tree == null) {
                return;
            }

            switch (_progress = tree.GetFloat("currenthealth") / tree.GetFloat("maxhealth")) {
                case <= 0.25f:
                    _color.Set(0.75f, 0.5f, 0.5f, _alpha);
                    break;
                case <= 0.5f:
                    _color.Set(0.75f, 0.75f, 0.5f, _alpha);
                    break;
                default:
                    _color.Set(0.5f, 0.75f, 0.5f, _alpha);
                    break;
            }
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

            // render background mesh
            _mvMatrix.Set(_api.Render.CurrentModelviewMatrix).Translate(x, y, 20).Scale(w, h, 0).Translate(0.5f, 0.5f, 0).Scale(0.5f, 0.5f, 0);
            shader.UniformMatrix("projectionMatrix", _api.Render.CurrentProjectionMatrix);
            shader.UniformMatrix("modelViewMatrix", _mvMatrix.Values);
            _api.Render.RenderMesh(_bgMesh);

            // render foreground mesh
            _mvMatrix.Set(_api.Render.CurrentModelviewMatrix).Translate(x, y, 20).Scale(w * _progress, h, 0).Translate(0.5f, 0.5f, 0).Scale(0.5f, 0.5f, 0);
            shader.UniformMatrix("projectionMatrix", _api.Render.CurrentProjectionMatrix);
            shader.UniformMatrix("modelViewMatrix", _mvMatrix.Values);
            _api.Render.RenderMesh(_fgMesh);
        }

        public void Dispose() {
            _api.Render.DeleteMesh(_bgMesh);
            _api.Render.DeleteMesh(_fgMesh);
            _api.Event.UnregisterRenderer(this, EnumRenderStage.Ortho);
        }
    }
}
