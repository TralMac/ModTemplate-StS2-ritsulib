using MegaCrit.Sts2.Core.Nodes;
using STS2RitsuLib.Patching.Models;

namespace RitsuContent.RitsuContentCode.Patches;

/// <summary>
/// Example RitsuLib patch: logs once when the game checks whether it is a release build.
/// A patch declares its targets in GetTargets and its Harmony Prefix/Postfix/Transpiler as static methods.
/// Feel free to delete this class (and its registration in ModPatches).
/// </summary>
public sealed class ExamplePatch : IPatchMethod
{
    private static bool _logged;

    //Unique id of the patch, used in logs and diagnostics.
    public static string PatchId => "ritsucontent_example_patch";

    public static string Description => "Log NGame.IsReleaseGame once";

    //Non-critical patches only log a warning when they fail to apply.
    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
        [new(typeof(NGame), nameof(NGame.IsReleaseGame), ignoreIfMissing: true)];

    public static void Postfix(bool __result)
    {
        if (_logged) return;
        _logged = true;
        MainFile.Logger.Info($"NGame.IsReleaseGame = {__result}");
    }
}
