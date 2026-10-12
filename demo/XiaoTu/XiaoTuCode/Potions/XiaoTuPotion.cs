using XiaoTu.XiaoTuCode.Character;
using XiaoTu.XiaoTuCode.Extensions;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace XiaoTu.XiaoTuCode.Potions;

/// <summary>
/// Base class for your character's potions. Every concrete subclass is registered into XiaoTuPotionPool.
/// Localization key: XIAO_TU_POTION_{CLASS_NAME_IN_UPPER_SNAKE_CASE} in potions.json.
/// </summary>
[RegisterPotion(typeof(XiaoTuPotionPool), Inherit = true)]
public abstract class XiaoTuPotion : ModPotionTemplate
{
    //Loads XiaoTu/images/potions/{ClassName}.png and potions/outline/{ClassName}.png.
    public override PotionAssetProfile AssetProfile => new(
        ImagePath: $"potions/{GetType().Name}.png".ImagePathOr("potions/potion.png"),
        OutlinePath: $"potions/outline/{GetType().Name}.png".ImagePathOr("potions/outline/potion.png")
    );
}
