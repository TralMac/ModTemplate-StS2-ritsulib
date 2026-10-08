using MegaCrit.Sts2.Core.Entities.Cards;
using RitsuContent.RitsuContentCode.Extensions;
using STS2RitsuLib.Scaffolding.Content;

namespace RitsuContent.RitsuContentCode.Cards;

/// <summary>
/// Base class for your mod's cards. Each card chooses its pool with [RegisterCard(typeof(SomeCardPool))],
/// e.g. ColorlessCardPool or a character's pool such as IroncladCardPool.
/// (If all your cards share one pool, put [RegisterCard(typeof(ThatPool), Inherit = true)] on this class instead.)
///
/// RitsuLib gives each card the public entry RITSU_CONTENT_CARD_{CLASS_NAME_IN_UPPER_SNAKE_CASE},
/// which is also its localization key in localization/{language}/cards.json.
/// </summary>
public abstract class RitsuContentCard(int cost, CardType type, CardRarity rarity, TargetType target) :
    ModCardTemplate(cost, type, rarity, target)
{
    //Loads RitsuContent/images/cards/{ClassName}.png. The vanilla portrait size is 250x190 (250x351 for ancient cards).
    //When the image does not exist, RitsuLib's built-in placeholder portrait is used.
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"cards/{GetType().Name}.png".ImagePathOr(null)
    );
}
