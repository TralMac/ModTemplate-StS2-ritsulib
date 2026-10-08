using System.Reflection;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using RitsuMod.RitsuModCode.Patches;
using STS2RitsuLib;
using STS2RitsuLib.Interop;
using STS2RitsuLib.Patching.Core;

namespace RitsuMod.RitsuModCode;

//You're recommended but not required to keep all your code in this package and all your assets in the RitsuMod folder.
[ModInitializer(nameof(Initialize))]
public static class MainFile
{
    //Must match the "id" in RitsuMod.json. RitsuLib uses it to build public entries such as RITSU_MOD_CARD_MY_CARD.
    public const string ModId = "RitsuMod";
    public const string ResPath = $"res://{ModId}";

    public static Logger Logger { get; } = RitsuLibFramework.CreateLogger(ModId);

    public static void Initialize()
    {
        var assembly = Assembly.GetExecutingAssembly();

        //Lets RitsuLib find the [RegisterCard], [RegisterRelic], [RegisterPower]... attributes in this assembly.
        ModTypeDiscoveryHub.RegisterModAssembly(ModId, assembly);

        //If you attach C# scripts defined in your mod to Godot scenes, uncomment the following line.
        //RitsuLibFramework.EnsureGodotScriptsRegistered(assembly, Logger);

        //RitsuLib patch system. Add your patches to ModPatches. Plain Harmony (new Harmony(ModId).PatchAll(assembly)) also works.
        var patcher = RitsuLibFramework.CreatePatcher(ModId, "main");
        patcher.RegisterPatches<ModPatches>();
        RitsuLibFramework.ApplyRequiredPatcher(patcher,
            () => Logger.Error($"{ModId}: a critical patch failed to apply, some features will not work."));

        //Content can be registered with attributes on the model classes, e.g.
        //[RegisterCard(typeof(ColorlessCardPool))] public sealed class MyCard() : ModCardTemplate(...)
        //or as a batch here:
        //RitsuLibFramework.CreateContentPack(ModId)
        //    .Card<ColorlessCardPool, MyCard>()
        //    .Apply();

        Logger.Info($"{ModId} initialized.");
    }
}
