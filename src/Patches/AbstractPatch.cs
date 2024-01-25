using System;
using System.Reflection;
using HarmonyLib;

namespace Pl3xTweaks.Patches;

public abstract class AbstractPatch {
    private readonly Harmony _harmony;

    protected AbstractPatch(Harmony harmony) {
        _harmony = harmony;
    }

    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

    protected void Patch<T>(string original, Delegate? prefix = null, Delegate? postfix = null, Delegate? transpiler = null, Delegate? finalizer = null) {
        MethodInfo? method = typeof(T).GetMethod(original, Flags);
        if (prefix != null) {
            _harmony.Patch(method, prefix: prefix);
        }

        if (postfix != null) {
            _harmony.Patch(method, postfix: postfix);
        }

        if (transpiler != null) {
            _harmony.Patch(method, transpiler: transpiler);
        }

        if (finalizer != null) {
            _harmony.Patch(method, finalizer: finalizer);
        }
    }
}
