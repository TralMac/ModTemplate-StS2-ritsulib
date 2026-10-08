using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using RitsuChar.RitsuCharCode.Powers;

namespace RitsuChar.RitsuCharCode.Cards;

//Not a starter card: it only shows up in card rewards and shops.
public sealed class ExamplePowerCard() : RitsuCharCard(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    //{ExamplePower} in the localization refers to this PowerVar (its name is the power's class name).
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<ExamplePower>(1)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<ExamplePower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(ExamplePower)].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[nameof(ExamplePower)].UpgradeValueBy(1);
    }
}
