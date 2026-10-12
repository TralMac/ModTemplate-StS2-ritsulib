using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using XiaoTu.XiaoTuCode.Dice;

namespace XiaoTu.XiaoTuCode.Powers;

/// <summary>财大气粗: +1 attack damage per 100 gold, up to Amount.</summary>
public sealed class WealthyPower : XiaoTuPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer,
        MegaCrit.Sts2.Core.Models.CardModel? cardSource, CardPlay? cardPlay)
    {
        if (dealer != Owner || Owner.Player == null || !props.IsPoweredAttack()) return 0m;
        return Math.Min(Amount, Math.Max(0, Owner.Player.Gold) / 100);
    }
}
