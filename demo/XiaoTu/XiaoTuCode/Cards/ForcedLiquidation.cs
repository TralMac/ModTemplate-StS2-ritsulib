using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using XiaoTu.XiaoTuCode.Dice;

namespace XiaoTu.XiaoTuCode.Cards;

public sealed class ForcedLiquidation() : XiaoTuCard(3, CardType.Attack, CardRarity.Common, TargetType.AllEnemies)
{
    //Live value on the card face: base + extra x (dice-based multiplier), recalculated while the card is in hand.
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(12m),
        new ExtraDamageVar(1m),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((card, _) => DiceState.Of(card.Owner).Total)
    ];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var damage = DynamicVars.CalculatedDamage.Calculate(null);
        await Hit(ctx, play, damage);
        Dice.Pledge(Dice.Count, IsUpgraded ? 2 : 3);
    }

    protected override void OnUpgrade()
    {
        //Upgraded: pledge duration 3 -> 2 (checked through IsUpgraded in OnPlay).
    }
}
