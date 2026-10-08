using Godot;
using RitsuChar.RitsuCharCode.Extensions;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Utils;

namespace RitsuChar.RitsuCharCode.Character;

public sealed class RitsuCharCardPool : TypeListCardPoolModel
{
    private static Material? _frameMaterial;

    //Pool id, not a display name. Must be unique.
    public override string Title => "ritsuchar";

    //Also used as the id of this pool's energy icons, e.g. in card descriptions.
    public override string EnergyColorName => "ritsuchar";

    //Energy icon used in card descriptions (24x24).
    public override string? TextEnergyIconPath => "charui/text_energy.png".ImagePath();

    //Energy icon used on the card's cost and in tooltips (74x74).
    public override string? BigEnergyIconPath => "charui/big_energy.png".ImagePath();

    //Color of small card icons, e.g. in the deck view.
    public override Color DeckEntryCardColor => RitsuCharCharacter.ThemeColor;

    //Outline color of the energy counter text.
    public override Color EnergyOutlineColor => RitsuCharCharacter.ThemeColor.Darkened(0.5f);

    //Recolors the vanilla card frames for every card in this pool.
    //If you draw your own card frames (CardAssetProfile.FramePath), use MaterialUtils.CreateUnmodulatedHsvShaderMaterial() instead.
    public override Material? PoolFrameMaterial => _frameMaterial ??= MaterialUtils.CreateReplaceHueShaderMaterial(
        RitsuCharCharacter.ThemeColor.R, RitsuCharCharacter.ThemeColor.G, RitsuCharCharacter.ThemeColor.B);

    public override bool IsColorless => false;
}
