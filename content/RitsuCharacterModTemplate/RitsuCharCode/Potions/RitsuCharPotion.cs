using RitsuChar.RitsuCharCode.Character;
using RitsuChar.RitsuCharCode.Extensions;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace RitsuChar.RitsuCharCode.Potions;

/// <summary>
/// Base class for your character's potions. Every concrete subclass is registered into RitsuCharPotionPool.
/// Localization key: RITSU_CHAR_POTION_{CLASS_NAME_IN_UPPER_SNAKE_CASE} in potions.json.
/// </summary>
[RegisterPotion(typeof(RitsuCharPotionPool), Inherit = true)]
public abstract class RitsuCharPotion : ModPotionTemplate
{
    //Loads RitsuChar/images/potions/{ClassName}.png and potions/outline/{ClassName}.png.
    public override PotionAssetProfile AssetProfile => new(
        ImagePath: $"potions/{GetType().Name}.png".ImagePathOr("potions/potion.png"),
        OutlinePath: $"potions/outline/{GetType().Name}.png".ImagePathOr("potions/outline/potion.png")
    );
}
