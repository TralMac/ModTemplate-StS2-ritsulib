using RitsuContent.RitsuContentCode.Extensions;
using STS2RitsuLib.Scaffolding.Content;

namespace RitsuContent.RitsuContentCode.Relics;

/// <summary>
/// Base class for your mod's relics. Each relic chooses its pool with [RegisterRelic(typeof(SomeRelicPool))],
/// e.g. SharedRelicPool for relics every character can find.
/// Localization key: RITSU_CONTENT_RELIC_{CLASS_NAME_IN_UPPER_SNAKE_CASE} in relics.json.
/// </summary>
public abstract class RitsuContentRelic : ModRelicTemplate
{
    //Loads RitsuContent/images/relics/{ClassName}.png (85x85), {ClassName}_outline.png and big/{ClassName}.png (256x256).
    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"relics/{GetType().Name}.png".ImagePathOr("relics/relic.png"),
        IconOutlinePath: $"relics/{GetType().Name}_outline.png".ImagePathOr("relics/relic_outline.png"),
        BigIconPath: $"relics/big/{GetType().Name}.png".ImagePathOr("relics/big/relic.png")
    );
}
