using System.IO;
using pl3xtweaks;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace Pl3xTweaks.configuration;

public class FileWatcher {
    private readonly FileSystemWatcher _watcher;
    private bool _queued;
    private readonly ICoreAPI _api;

    public FileWatcher(ICoreAPI api) {
        _api = api;

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
        QueueReload();
    }

    private void Error(object sender, ErrorEventArgs e) {
        QueueReload();
    }

    /// <summary>
    /// My workaround for <a href='https://github.com/dotnet/runtime/issues/24079'>dotnet#24079</a>.
    /// </summary>
    private void QueueReload() {
        // check if already queued for reload
        if (_queued) {
            return;
        }

        // mark as queued
        _queued = true;

        // wait for other changes to process
        _api.Event.RegisterCallback(_ => {
            TweaksMod.Logger.Event("Detected changes to the config. Reloading...");

            // reload the config
            Config.Reload();

            // wait some more to remove this change from the queue since the reload triggers another write
            _api.Event.RegisterCallback(_ => {
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
