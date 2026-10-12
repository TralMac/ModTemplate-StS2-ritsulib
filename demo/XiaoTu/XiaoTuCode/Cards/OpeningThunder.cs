using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using XiaoTu.XiaoTuCode.Dice;

namespace XiaoTu.XiaoTuCode.Cards;

public sealed class OpeningThunder() : XiaoTuCard(3, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
    //Live value on the card face: base + extra x (dice-based multiplier), recalculated while the card is in hand.
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(0m),
        new ExtraDamageVar(1m),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((card, _) => DiceState.Of(card.Owner).Total * DiceState.Of(card.Owner).PairedCount)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Innate];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        while (Dice.Count < Dice.Capacity) Dice.Add(temporary: true);
        await Hit(ctx, play, DynamicVars.CalculatedDamage.Calculate(play.Target));
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
