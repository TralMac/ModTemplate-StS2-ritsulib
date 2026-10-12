using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using XiaoTu.XiaoTuCode.Dice;
using XiaoTu.XiaoTuCode.Powers;

namespace XiaoTu.XiaoTuCode.Cards;

public sealed class GoddessLuo() : XiaoTuCard(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var limit = IsUpgraded ? 99 : 6;
        for (var i = 0; i < limit; i++)
        {
            Dice.RerollLowest();
            if (Dice.Lowest >= 6 || Dice.Count == 0) break;
        }
        await Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        //The upgraded effect is checked through IsUpgraded in OnPlay.
    }
}
