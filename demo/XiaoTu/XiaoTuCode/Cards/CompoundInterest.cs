using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using XiaoTu.XiaoTuCode.Dice;

namespace XiaoTu.XiaoTuCode.Cards;

public sealed class CompoundInterest() : XiaoTuCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    //Live value on the card face: base + extra x (dice-based multiplier), recalculated while the card is in hand.
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(9m),
        new ExtraDamageVar(3m),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((card, _) => DiceState.Of(card.Owner).GainOrPledgeCount)
    ];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await Hit(ctx, play, DynamicVars.CalculatedDamage.Calculate(play.Target));
    }

    protected override void OnUpgrade()
    {
        DynamicVars["ExtraDamage"].UpgradeValueBy(1m);
    }
}
