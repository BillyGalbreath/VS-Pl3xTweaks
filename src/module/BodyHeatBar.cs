using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace pl3xtweaks.module;

public class BodyHeatBar(Pl3xTweaks __mod) : Module(__mod) {
    public override void StartClientSide(ICoreClientAPI capi) {
        capi.Gui.RegisterDialog(new BodyHeatElement(capi));
    }

    private class BodyHeatElement(ICoreClientAPI __capi) : HudElement(__capi) {
        private readonly ElementBounds _bounds = ElementBounds.Fixed(248, -95, 348, 10);
        private BodyHeatStatBar? _heatbar;
        private long _listenerId;

        public override void OnOwnPlayerDataReceived() {
            SingleComposer = capi.Gui
                .CreateCompo("bodyheat", new ElementBounds {
                    Alignment = EnumDialogArea.CenterBottom,
                    BothSizing = ElementSizing.Fixed,
                    fixedWidth = _bounds.fixedWidth,
                    fixedHeight = _bounds.fixedHeight
                })
                .AddInteractiveElement(_heatbar = new BodyHeatStatBar(capi, _bounds))
                .Compose();

            TryOpen();

            _listenerId = capi.Event.RegisterGameTickListener(_ => {
                float bodytemp = capi.World.Player.Entity?.WatchedAttributes.GetTreeAttribute("bodyTemp")?.GetFloat("bodytemp") ?? 0;
                _heatbar?.SetValue(bodytemp);
            }, 200);
        }

        public override void Dispose() {
            capi.Event.UnregisterGameTickListener(_listenerId);
            base.Dispose();
            _heatbar = null;
        }

        public override void OnRenderGUI(float deltaTime) {
            if (capi.World.Player.WorldData.CurrentGameMode != EnumGameMode.Spectator) {
                base.OnRenderGUI(deltaTime);
            }
        }

        public override double InputOrder => 1.0;

        public override string ToggleKeyCombinationCode => null!;

        public override bool Focusable => false;

        public override bool TryClose() => false;

        public override bool ShouldReceiveKeyboardEvents() => false;

        protected override void OnFocusChanged(bool on) { }

        public override void OnMouseDown(MouseEvent args) { }
    }

    private class BodyHeatStatBar : GuiElementStatbar {
        private const float _minTemp = 33;
        private const float _maxTemp = 37;
        private const float _flashTemp = 34.5f;
        private float _curTemp;

        public BodyHeatStatBar(ICoreClientAPI capi, ElementBounds bounds) : base(capi, bounds, ColorUtil.Hex2Doubles("#81C0CC"), true, true) {
            SetLineInterval(0.5f);
            HideWhenFull = true;
            onGetStatbarValue = () => $"{(_curTemp > _maxTemp ? _maxTemp + (_curTemp - _maxTemp) / 10 : _curTemp):0.#}°C";
        }

        public override void RenderInteractiveElements(float deltaTime) {
            if (!HideWhenFull || _curTemp < _maxTemp) {
                base.RenderInteractiveElements(deltaTime);
            }
        }

        public new void SetValue(float value) {
            if (Math.Abs(_curTemp - value) >= 0.001) {
                _curTemp = value;
                SetValues(_curTemp - _minTemp, _minTemp, _maxTemp);
                ShouldFlash = _curTemp <= _flashTemp;
            }
        }
    }
}
