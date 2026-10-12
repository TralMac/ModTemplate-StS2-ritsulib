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

public sealed class GambleLoserPower : XiaoTuPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner?.Creature != Owner || cardPlay.Card.Type != CardType.Attack || Owner.Player == null) return;
        var ctx = new ThrowingPlayerChoiceContext();
        var dice = DiceState.Of(Owner.Player);
        var ok = Amount >= 2 ? dice.BetHighLowest() : dice.BetHigh();
        Flash();
        if (ok && cardPlay.Target != null) await PowerCmd.Apply<VulnerablePower>(ctx, cardPlay.Target, 1, Owner, cardPlay.Card);
        else if (!ok) await PowerCmd.Apply<WeakPower>(ctx, Owner, 1, Owner, cardPlay.Card);
        await DiceDispatcher.Dispatch(ctx, Owner.Player);
    }
}
