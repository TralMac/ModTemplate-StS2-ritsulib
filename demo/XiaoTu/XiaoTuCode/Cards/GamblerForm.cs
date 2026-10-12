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

public sealed class GamblerForm() : XiaoTuCard(3, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Amount", 4m)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        Dice.Floor = Math.Max(Dice.Floor, DynamicVars["Amount"].IntValue);
        await PowerCmd.Apply<GamblerFormPower>(ctx, Owner.Creature, DynamicVars["Amount"].BaseValue, Owner.Creature, this);
        Dice.Add(temporary: false);
        Dice.RerollAll();
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Amount"].UpgradeValueBy(1m);
    }
}
