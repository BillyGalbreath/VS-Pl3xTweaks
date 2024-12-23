using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace pl3xtweaks.module;

public abstract class Module {
    protected readonly Pl3xTweaks _mod;

    protected Module(Pl3xTweaks mod) {
        _mod = mod;
    }

    //public virtual void StartPre(ICoreAPI api) { }

    public virtual void Start(ICoreAPI api) { }

    //public virtual void AssetsLoaded(ICoreAPI api) { }

    public virtual void AssetsFinalize(ICoreAPI api) { }

    public virtual void StartClientSide(ICoreClientAPI api) { }

    public virtual void StartServerSide(ICoreServerAPI api) { }

    public virtual void Reload() { }

    public virtual void Dispose() { }
}
