using System.Reflection;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using RitsuChar.RitsuCharCode.Patches;
using STS2RitsuLib;
using STS2RitsuLib.Interop;
using STS2RitsuLib.Patching.Core;

namespace RitsuChar.RitsuCharCode;

//You're recommended but not required to keep all your code in this package and all your assets in the RitsuChar folder.
[ModInitializer(nameof(Initialize))]
public static class MainFile
{
    //Must match the "id" in RitsuChar.json. RitsuLib uses it to build public entries such as RITSU_CHAR_CARD_STRIKE_RITSU_CHAR.
    public const string ModId = "RitsuChar";
    public const string ResPath = $"res://{ModId}";

    public static Logger Logger { get; } = RitsuLibFramework.CreateLogger(ModId);

    public static void Initialize()
    {
        var assembly = Assembly.GetExecutingAssembly();

        //Lets RitsuLib find the [RegisterCard], [RegisterRelic], [RegisterCharacter]... attributes in this assembly.
        ModTypeDiscoveryHub.RegisterModAssembly(ModId, assembly);

        //If you attach C# scripts defined in your mod to Godot scenes, uncomment the following line.
        //RitsuLibFramework.EnsureGodotScriptsRegistered(assembly, Logger);

        //RitsuLib patch system. Add your patches to ModPatches. Plain Harmony (new Harmony(ModId).PatchAll(assembly)) also works.
        var patcher = RitsuLibFramework.CreatePatcher(ModId, "main");
        patcher.RegisterPatches<ModPatches>();
        RitsuLibFramework.ApplyRequiredPatcher(patcher,
            () => Logger.Error($"{ModId}: a critical patch failed to apply, some features will not work."));

        //Registration can also be written as a batch instead of attributes, for example:
        //RitsuLibFramework.CreateContentPack(ModId)
        //    .Card<RitsuCharCardPool, SomeCard>()
        //    .Apply();

        Logger.Info($"{ModId} initialized.");
    }
}
