using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using XiaoTu.XiaoTuCode.Extensions;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;

namespace XiaoTu.XiaoTuCode.Character;

/// <summary>
/// Your character. Localization key: XIAO_TU_CHARACTER_XIAO_TU_CHARACTER in characters.json.
///
/// The starting deck and relics are not listed here: cards and relics add themselves with
/// [RegisterCharacterStarterCard(typeof(XiaoTuCharacter), count)] and [RegisterCharacterStarterRelic(typeof(XiaoTuCharacter))].
/// </summary>
[RegisterCharacter]
public sealed class XiaoTuCharacter : ModCharacterTemplate<XiaoTuCardPool, XiaoTuRelicPool, XiaoTuPotionPool>
{
    public static readonly Color ThemeColor = new("8c7bff");

    public override Color NameColor => ThemeColor;
    public override Color EnergyLabelOutlineColor => ThemeColor.Darkened(0.5f);
    public override Color MapDrawingColor => ThemeColor;

    public override CharacterGender Gender => CharacterGender.Neutral;
    public override int StartingHp => 70;
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
        //    VisualsPath: "res://XiaoTu/scenes/character.tscn",
        //    EnergyCounterPath: "res://XiaoTu/scenes/energy_counter.tscn",
        //    MerchantAnimPath: "res://XiaoTu/scenes/merchant.tscn",
        //    RestSiteAnimPath: "res://XiaoTu/scenes/rest_site.tscn"
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
