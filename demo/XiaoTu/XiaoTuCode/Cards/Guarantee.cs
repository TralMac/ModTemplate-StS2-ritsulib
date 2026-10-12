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

public sealed class Guarantee() : XiaoTuCard(2, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await PowerCmd.Apply<GuaranteePower>(ctx, Owner.Creature, 1, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        //The upgraded effect is checked through IsUpgraded in OnPlay.
    }
}
