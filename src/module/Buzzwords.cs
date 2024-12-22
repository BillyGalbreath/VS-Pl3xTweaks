using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.Client.NoObf;

namespace pl3xtweaks.module;

public class Buzzwords : Module {
    private static ICoreClientAPI? _api;
    private static double? _closestDistance;

    private const int _radius = 15;

    private long _taskId;

    public Buzzwords(Pl3xTweaks mod) : base(mod) { }

    public override void StartClientSide(ICoreClientAPI api) {
        _api = api;
        _taskId = api.Event.RegisterGameTickListener(OnTick, 1000);
        _mod.Patch<SystemClientTickingBlocks>("OnSeperateThreadGameTick", postfix: Postfix);
    }

    private static void OnTick(float _) {
        // set a high number to inform the patch to check distance
        _closestDistance = 99999;
    }

    private static void Postfix(ClientMain ___game, Dictionary<Vec3i, Dictionary<AssetLocation, AmbientSound>> ___currentAmbientSoundsBySection) {
        // only do a distance check when we want to
        if (_closestDistance == null) {
            return;
        }

        // get the player's current position
        BlockPos? pos = ___game.Player?.Entity?.Pos?.AsBlockPos;
        if (pos == null) {
            return;
        }

        // iterate all ambient sounds
        ___currentAmbientSoundsBySection.Foreach(sections => {
            sections.Value.Foreach(pair => {
                // look for a beehive sound
                if (pair.Key.ToString().Contains("beehive")) {
                    pair.Value.BoundingBoxes.Foreach(box => {
                        // check the distance to the center cuboid
                        Vec3i center = box.Center;
                        double distance = pos.DistanceTo(center.X, center.Y, center.Z);
                        if (distance < _closestDistance) {
                            // we only care if it's closer than the last once
                            _closestDistance = distance;
                        }
                    });
                }
            });
        });

        // is the closest distance within the acceptable radius
        if (_closestDistance <= _radius) {
            // number of zZ's to put in the message
            int count = (int)Math.Min(_radius + 3, Math.Max(_radius - _closestDistance.Value, 3));
            // construct the BuzZ
            string str = "B " + string.Concat(Enumerable.Repeat<string>("z Z ", count)) + "z . . .";
            // format the BuzZ
            string message = $"<strong><font size=\"20\" color=\"yellow\">{str}</font></strong>";
            // hop over to the main thread, so we can draw on the gui
            _api!.Event.EnqueueMainThreadTask(() => {
                // draw error message on the gui
                _api.TriggerIngameError(null, "buzz", message);
            }, "buzz");
        }

        // we're done now. wait for next tick
        _closestDistance = null;
    }

    public override void Dispose() {
        _api?.Event.UnregisterGameTickListener(_taskId);
    }
}
