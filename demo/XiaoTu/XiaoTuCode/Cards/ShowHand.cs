using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using XiaoTu.XiaoTuCode.Dice;
using XiaoTu.XiaoTuCode.Powers;

namespace XiaoTu.XiaoTuCode.Cards;

public sealed class ShowHand() : XiaoTuCard(0, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Amount", 15m)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var gold = Owner.Gold;
        if (gold <= 0) return;
        var pct = DynamicVars["Amount"].BaseValue + gold / 2;
        await LoseGold(gold);
        await PowerCmd.Apply<AllOutPower>(ctx, Owner.Creature, pct, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Amount"].UpgradeValueBy(15m);
    }
}
