using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using XiaoTu.XiaoTuCode.Dice;

namespace XiaoTu.XiaoTuCode.Powers;

public sealed class BankrollPower : XiaoTuPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (target != Owner || Owner.Player == null || !props.IsPoweredAttack()) return 0m;
        var avail = Owner.Player.Gold - GoldOps.Floor;
        return -Math.Min(Math.Min(Amount, amount), Math.Max(0, avail));
    }

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner || Owner.Player == null || !props.IsPoweredAttack()) return;
        var pay = Math.Min(Amount, Math.Max(0, Owner.Player.Gold - GoldOps.Floor));
        if (pay <= 0) return;
        Flash();
        GoldOps.Lose(Owner.Player, pay);
        await DiceDispatcher.Dispatch(choiceContext, Owner.Player);
    }
}
