using MegaCrit.Sts2.Core.Entities.Cards;
using RitsuChar.RitsuCharCode.Character;
using RitsuChar.RitsuCharCode.Extensions;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RitsuChar.RitsuCharCode.Cards;

/// <summary>
/// Base class for your character's cards.
/// Inherit = true makes every concrete subclass register itself into RitsuCharCardPool automatically.
/// A subclass can declare its own [RegisterCard(typeof(OtherPool))] to go into a different pool instead.
///
/// RitsuLib gives each card the public entry RITSU_CHAR_CARD_{CLASS_NAME_IN_UPPER_SNAKE_CASE},
/// which is also its localization key in localization/{language}/cards.json.
/// </summary>
[RegisterCard(typeof(RitsuCharCardPool), Inherit = true)]
public abstract class RitsuCharCard(int cost, CardType type, CardRarity rarity, TargetType target) :
    ModCardTemplate(cost, type, rarity, target)
{
    //Loads RitsuChar/images/cards/{ClassName}.png. The vanilla portrait size is 250x190 (250x351 for ancient cards).
    //When the image does not exist, RitsuLib's built-in placeholder portrait is used.
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"cards/{GetType().Name}.png".ImagePathOr(null)
    );
}
