using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using RitsuChar.RitsuCharCode.Extensions;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;

namespace RitsuChar.RitsuCharCode.Character;

/// <summary>
/// Your character. Localization key: RITSU_CHAR_CHARACTER_RITSU_CHAR_CHARACTER in characters.json.
///
/// The starting deck and relics are not listed here: cards and relics add themselves with
/// [RegisterCharacterStarterCard(typeof(RitsuCharCharacter), count)] and [RegisterCharacterStarterRelic(typeof(RitsuCharCharacter))].
/// </summary>
[RegisterCharacter]
public sealed class RitsuCharCharacter : ModCharacterTemplate<RitsuCharCardPool, RitsuCharRelicPool, RitsuCharPotionPool>
{
    public static readonly Color ThemeColor = new("8c7bff");

    public override Color NameColor => ThemeColor;
    public override Color EnergyLabelOutlineColor => ThemeColor.Darkened(0.5f);
    public override Color MapDrawingColor => ThemeColor;

    public override CharacterGender Gender => CharacterGender.Neutral;
    public override int StartingHp => 75;
    public override int StartingGold => 99;

    //Delays (in seconds) used to sync attack/cast effects with the character animation.
    public override float AttackAnimDelay => 0.15f;
    public override float CastAnimDelay => 0.25f;

    //Set to true only if you register a story/epochs for this character (see the RitsuLib timeline docs).
    public override bool RequiresEpochAndTimeline => false;

    //Any asset not set here is borrowed from this vanilla character (combat visuals, energy counter, rest site, merchant, sfx...).
    //Replace them with your own before releasing; see CharacterAssetProfile for everything that can be set.
    public override string? PlaceholderCharacterId => "ironclad";

    public override CharacterAssetProfile AssetProfile => new(
        //Scenes: new(
        //    VisualsPath: "res://RitsuChar/scenes/character.tscn",
        //    EnergyCounterPath: "res://RitsuChar/scenes/energy_counter.tscn",
        //    MerchantAnimPath: "res://RitsuChar/scenes/merchant.tscn",
        //    RestSiteAnimPath: "res://RitsuChar/scenes/rest_site.tscn"
        //),
        Ui: new(
            //Small icon used in the top bar and run history.
            IconTexturePath: "charui/character_icon.png".ImagePath(),
            CharacterSelectIconPath: "charui/char_select.png".ImagePath(),
            CharacterSelectLockedIconPath: "charui/char_select_locked.png".ImagePath(),
            MapMarkerPath: "charui/map_marker.png".ImagePath()
        )
    );

    //Visual effects used when this character attacks the Architect.
    public override List<string> GetArchitectAttackVfx() =>
    [
        "vfx/vfx_attack_blunt",
        "vfx/vfx_heavy_blunt",
        "vfx/vfx_attack_slash",
        "vfx/vfx_bloody_impact",
        "vfx/vfx_rock_shatter"
    ];
}
