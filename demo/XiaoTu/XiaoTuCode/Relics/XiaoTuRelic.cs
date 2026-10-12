using XiaoTu.XiaoTuCode.Character;
using XiaoTu.XiaoTuCode.Extensions;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace XiaoTu.XiaoTuCode.Relics;

/// <summary>
/// Base class for your character's relics. Every concrete subclass is registered into XiaoTuRelicPool.
/// Localization key: XIAO_TU_RELIC_{CLASS_NAME_IN_UPPER_SNAKE_CASE} in relics.json.
/// </summary>
[RegisterRelic(typeof(XiaoTuRelicPool), Inherit = true)]
public abstract class XiaoTuRelic : ModRelicTemplate
{
    //Loads XiaoTu/images/relics/{ClassName}.png (85x85), {ClassName}_outline.png and big/{ClassName}.png (256x256).
    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"relics/{GetType().Name}.png".ImagePathOr("relics/relic.png"),
        IconOutlinePath: $"relics/{GetType().Name}_outline.png".ImagePathOr("relics/relic_outline.png"),
        BigIconPath: $"relics/big/{GetType().Name}.png".ImagePathOr("relics/big/relic.png")
    );
}
