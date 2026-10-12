using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using XiaoTu.XiaoTuCode.Character;
using XiaoTu.XiaoTuCode.Dice;

namespace XiaoTu.XiaoTuCode.Cards;

[RegisterCharacterStarterCard(typeof(XiaoTuCharacter), 1, Order = 20)]
public sealed class RustyDice() : XiaoTuCard(0, CardType.Skill, CardRarity.Basic, TargetType.Self)
{
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        DiceState.Of(Owner).Add(temporary: false);
        await Task.CompletedTask;
    }
}
