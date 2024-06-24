namespace pl3xtweaks.module;

public abstract class Module {
    protected readonly Pl3xTweaks _mod;

    protected Module(Pl3xTweaks mod) {
        _mod = mod;
    }

    public virtual void Dispose() { }
}
