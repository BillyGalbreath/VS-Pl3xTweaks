using System.IO;
using Vintagestory.API.Config;

namespace Pl3xTweaks.Configuration;

public class FileWatcher {
    private readonly FileSystemWatcher _watcher;
    private bool _queued;

    public FileWatcher() {
        _watcher = new FileSystemWatcher(GamePaths.ModConfig);

        _watcher.Filter = $"{TweaksMod.Id}.yml";
        _watcher.IncludeSubdirectories = false;
        _watcher.EnableRaisingEvents = true;

        _watcher.Changed += Changed;
        _watcher.Created += Changed;
        _watcher.Deleted += Changed;
        _watcher.Renamed += Changed;
        _watcher.Error += Error;
    }

    private void Changed(object sender, FileSystemEventArgs e) {
        QueueReload(e.ChangeType.ToString().ToLowerInvariant());
    }

    private void Error(object sender, ErrorEventArgs e) {
        TweaksMod.Logger.Error(e.GetException().ToString());
        QueueReload();
    }

    /// <summary>
    /// My workaround for <a href='https://github.com/dotnet/runtime/issues/24079'>dotnet#24079</a>.
    /// </summary>
    private void QueueReload(string? changeType = null) {
        // check if already queued for reload
        if (_queued) {
            return;
        }

        // mark as queued
        _queued = true;

        // inform console/log
        if (changeType != null) {
            TweaksMod.Logger.Event($"Detected the config was {changeType}");
        }

        // wait for other changes to process
        TweaksMod.Api?.Event.RegisterCallback(_ => {
            // reload the config
            Config.Reload();

            // wait some more to remove this change from the queue since the reload triggers another write
            TweaksMod.Api.Event.RegisterCallback(_ => {
                // unmark as queued
                _queued = false;
            }, 100);
        }, 100);
    }

    public void Dispose() {
        _watcher.Changed -= Changed;
        _watcher.Created -= Changed;
        _watcher.Deleted -= Changed;
        _watcher.Renamed -= Changed;
        _watcher.Error -= Error;

        _watcher.Dispose();
    }
}
