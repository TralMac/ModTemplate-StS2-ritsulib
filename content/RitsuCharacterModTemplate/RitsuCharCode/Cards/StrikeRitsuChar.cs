using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using RitsuChar.RitsuCharCode.Character;
using STS2RitsuLib.Interop.AutoRegistration;

namespace RitsuChar.RitsuCharCode.Cards;

//Adds 5 copies of this card to the starting deck. Lower Order values come first in the deck.
[RegisterCharacterStarterCard(typeof(RitsuCharCharacter), 5, Order = 0)]
public sealed class StrikeRitsuChar() : RitsuCharCard(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
{
    //Base values of the card. {Damage} in the localization refers to this DamageVar.
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(6, ValueProp.Move)];

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
