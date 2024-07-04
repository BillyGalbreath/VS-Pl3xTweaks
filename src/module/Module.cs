using Vintagestory.API.Client;
using Vintagestory.API.Common;

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

    public virtual void Dispose() { }
}
