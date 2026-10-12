using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using XiaoTu.XiaoTuCode.Character;

namespace XiaoTu.XiaoTuCode.Cards;

[RegisterCharacterStarterCard(typeof(XiaoTuCharacter), 1, Order = 20)]
public sealed class RustyDice() : XiaoTuCard(0, CardType.Skill, CardRarity.Basic, TargetType.Self)
{
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Dice.Add(temporary: false);
        if (IsUpgraded) Dice.RerollAll();
        EnergyCost.AddThisCombat(1);
        await Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        //Upgraded version also rerolls all dice (see OnPlay).
    }
}
