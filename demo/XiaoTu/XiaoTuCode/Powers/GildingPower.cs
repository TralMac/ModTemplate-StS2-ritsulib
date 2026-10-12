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

public sealed class GildingPower : XiaoTuPower, IDiceListener
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    private int _spent;

    public Task OnGoldLost(PlayerChoiceContext ctx, int amount)
    {
        _spent += amount;
        while (_spent >= 15)
        {
            _spent -= 15;
            var cards = PileType.Draw.GetPile(Owner.Player!).Cards.Where(c => c.IsUpgradable).ToList();
            if (cards.Count == 0) continue;
            Flash();
            CardCmd.Upgrade(cards[Random.Shared.Next(cards.Count)]);
        }
        return Task.CompletedTask;
    }
}
