using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using XiaoTu.XiaoTuCode.Dice;

namespace XiaoTu.XiaoTuCode.Powers;

/// <summary>焉知祸福: whenever you lose gold to an effect, deal that much damage to ALL enemies.</summary>
public sealed class FortuneAndMisfortunePower : XiaoTuPower, IDiceListener
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public async Task OnGoldLost(PlayerChoiceContext ctx, int amount)
    {
        var enemies = CombatState.HittableEnemies;
        if (enemies.Count == 0) return;
        Flash();
        await CreatureCmd.Damage(ctx, enemies, amount, ValueProp.Unpowered, Owner);
    }
}
