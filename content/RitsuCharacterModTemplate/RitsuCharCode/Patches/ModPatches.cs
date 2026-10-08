using STS2RitsuLib.Patching.Core;
using STS2RitsuLib.Patching.Models;

namespace RitsuChar.RitsuCharCode.Patches;

/// <summary>
/// Every patch registered here is applied by the "main" patcher created in MainFile.Initialize.
/// </summary>
public sealed class ModPatches : IModPatches
{
    public static void AddTo(ModPatcher patcher)
    {
        patcher.RegisterPatch<ExamplePatch>();
    }
}
