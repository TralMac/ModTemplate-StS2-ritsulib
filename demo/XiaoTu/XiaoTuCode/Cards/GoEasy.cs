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

public sealed class GoEasy() : XiaoTuCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Gold", 12m)];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await PowerCmd.Apply<WeakPower>(ctx, play.Target!, 1, Owner.Creature, this);
        await PowerCmd.Apply<ManglePower>(ctx, play.Target!, 2, Owner.Creature, this);
        await LoseGold(DynamicVars["Gold"].IntValue);
        await CardPileCmd.Add(this, PileType.Hand);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Gold"].UpgradeValueBy(-4m);
    }
}
