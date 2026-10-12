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

public sealed class SetUp() : XiaoTuCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var enemies = CombatState!.HittableEnemies.ToList();
        var ok = IsUpgraded ? Dice.BetHighLowest() : Dice.BetHigh();
        var kinds = ok ? new[] { 0, 1, 2 } : new[] { Random.Shared.Next(3) };
        foreach (var kind in kinds)
            foreach (var e in enemies)
            {
                if (kind == 0) await PowerCmd.Apply<VulnerablePower>(ctx, e, 1, Owner.Creature, this);
                else if (kind == 1) await PowerCmd.Apply<FrailPower>(ctx, e, 1, Owner.Creature, this);
                else await PowerCmd.Apply<ManglePower>(ctx, e, 1, Owner.Creature, this);
            }
    }

    protected override void OnUpgrade()
    {
        //The upgraded effect is checked through IsUpgraded in OnPlay.
    }
}
