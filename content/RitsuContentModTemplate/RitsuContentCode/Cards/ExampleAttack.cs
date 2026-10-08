using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace RitsuContent.RitsuContentCode.Cards;

//Adds this card to the colorless card pool.
[RegisterCard(typeof(ColorlessCardPool))]
public sealed class ExampleAttack() : RitsuContentCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    //Base values of the card. {Damage} in the localization refers to this DamageVar.
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(9, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
#if (LegacyAttackApi)
            .FromCard(this)
#else
            .FromCard(this, cardPlay)
#endif
            .Targeting(cardPlay.Target!)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}
