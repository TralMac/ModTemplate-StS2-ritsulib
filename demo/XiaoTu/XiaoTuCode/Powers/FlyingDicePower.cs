using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using XiaoTu.XiaoTuCode.Dice;

namespace XiaoTu.XiaoTuCode.Powers;

/// <summary>飞来横骰: deal Amount damage to a random enemy for each die rerolled.</summary>
public sealed class FlyingDicePower : XiaoTuPower, IDiceListener
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public async Task OnRerolls(PlayerChoiceContext ctx, int count)
    {
        for (var i = 0; i < count; i++)
        {
            var enemies = CombatState.HittableEnemies;
            if (enemies.Count == 0) return;
            var target = Owner.Player!.RunState.Rng.CombatTargets.NextItem(enemies);
            Flash();
            await CreatureCmd.Damage(ctx, target, Amount, ValueProp.Unpowered, Owner);
        }
    }
}
