using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using XiaoTu.XiaoTuCode.Dice;

namespace XiaoTu.XiaoTuCode.Powers;

/// <summary>气运护体: reduce attack damage taken by your lowest die.</summary>
public sealed class FortuneShieldPower : XiaoTuPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer,
        MegaCrit.Sts2.Core.Models.CardModel? cardSource, CardPlay? cardPlay)
    {
        if (target != Owner || Owner.Player == null || !props.IsPoweredAttack()) return 0m;
        return -DiceState.Of(Owner.Player).Lowest;
    }
}
