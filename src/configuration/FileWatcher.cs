﻿using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace pl3xtweaks.configuration;

public class FileWatcher {
    private readonly FileSystemWatcher _watcher;
    private readonly Pl3xTweaks _mod;
    private readonly ICoreServerAPI _api;

    public bool Queued { get; set; }

    public FileWatcher(Pl3xTweaks mod, ICoreServerAPI api) {
        _mod = mod;
        _api = api;

        _watcher = new FileSystemWatcher(GamePaths.ModConfig) {
            Filter = $"{mod.ModId}.json",
            IncludeSubdirectories = false,
            EnableRaisingEvents = true
        };

        _watcher.Changed += Changed;
        _watcher.Created += Changed;
        _watcher.Deleted += Changed;
        _watcher.Renamed += Changed;
        _watcher.Error += Error;
    }

    private void Changed(object sender, FileSystemEventArgs e) {
        QueueReload(true);
    }

    private void Error(object sender, ErrorEventArgs e) {
        _mod.Logger.Error(e.GetException().ToString());
        QueueReload();
    }

    /// <summary>
    /// My workaround for <a href='https://github.com/dotnet/runtime/issues/24079'>dotnet#24079</a>.
    /// </summary>
    private void QueueReload(bool changed = false) {
        // check if already queued for reload
        if (Queued) {
            return;
        }

        // mark as queued
        Queued = true;

        // inform console/log
        if (changed) {
            _mod.Logger.Event("Detected the config was changed. Reloading.");
        }

        // wait for other changes to process
        _api.Event.RegisterCallback(_ => {
            // reload the config
            _mod.ReloadServerData(_api);

            // wait some more to remove this change from the queue since the reload triggers another write
            _api.Event.RegisterCallback(_ => {
                // unmark as queued
                Queued = false;
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
