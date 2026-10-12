using MegaCrit.Sts2.Core.Entities.Powers;
using XiaoTu.XiaoTuCode.Extensions;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace XiaoTu.XiaoTuCode.Powers;

/// <summary>
/// Base class for your mod's powers. Every concrete subclass is registered automatically.
/// Localization key: XIAO_TU_POWER_{CLASS_NAME_IN_UPPER_SNAKE_CASE} in powers.json.
/// </summary>
[RegisterPower(Inherit = true)]
public abstract class XiaoTuPower : ModPowerTemplate
{
    //Loads XiaoTu/images/powers/{ClassName}.png (64x64) and big/{ClassName}.png (256x256).
    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"powers/{GetType().Name}.png".ImagePathOr("powers/power.png"),
        BigIconPath: $"powers/big/{GetType().Name}.png".ImagePathOr("powers/big/power.png")
    );

    /// <summary>
    /// Whether this power is a buff or debuff.
    /// </summary>
    public abstract override PowerType Type { get; }

    /// <summary>
    /// How this power stacks if reapplied. Counter is the most common type, where applying the power again just
    /// adds to the amount. Single means the power does not stack, like Barricade.
    /// </summary>
    public abstract override PowerStackType StackType { get; }
}
