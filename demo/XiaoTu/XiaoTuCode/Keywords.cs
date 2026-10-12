using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace XiaoTu.XiaoTuCode;

/// <summary>
/// Mod keywords shown as hover tips (side panel) next to a card, like the vanilla Vulnerable / Weak tips.
/// Texts live in localization/{lang}/card_keywords.json; <see cref="Cards.XiaoTuCard"/> attaches them to every card
/// whose description mentions the word (see CardKeywordMap).
/// </summary>
[RegisterOwnedCardKeyword("DICE", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.None, IncludeInCardHoverTip = true)]
[RegisterOwnedCardKeyword("TEMP_DICE", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.None, IncludeInCardHoverTip = true)]
[RegisterOwnedCardKeyword("TOTAL_DICE", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.None, IncludeInCardHoverTip = true)]
[RegisterOwnedCardKeyword("PLEDGE", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.None, IncludeInCardHoverTip = true)]
[RegisterOwnedCardKeyword("BET_HIGH", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.None, IncludeInCardHoverTip = true)]
[RegisterOwnedCardKeyword("REROLL", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.None, IncludeInCardHoverTip = true)]
public sealed class XiaoTuKeywords
{
}
