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

public sealed class WinStreakPower : XiaoTuPower, IDiceListener
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    private int _streak;

    public async Task OnBets(PlayerChoiceContext ctx, IReadOnlyList<bool> results)
    {
        foreach (var ok in results)
        {
            if (!ok) { _streak = 0; continue; }
            if (++_streak < 2) continue;
            _streak = 0;
            Flash();
            await PlayerCmd.GainEnergy(Amount, Owner.Player!);
        }
    }
}
