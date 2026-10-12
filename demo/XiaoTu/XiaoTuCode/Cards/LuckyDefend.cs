using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using XiaoTu.XiaoTuCode.Character;
using XiaoTu.XiaoTuCode.Dice;

namespace XiaoTu.XiaoTuCode.Cards;

public sealed class LuckyDefend() : XiaoTuCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(6, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var rolled = DiceState.Of(Owner).RerollRandom();
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block.BaseValue + rolled, ValueProp.Move, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3);
    }
}
