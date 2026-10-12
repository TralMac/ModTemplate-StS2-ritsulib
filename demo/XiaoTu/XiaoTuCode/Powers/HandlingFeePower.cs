using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using XiaoTu.XiaoTuCode.Dice;

namespace XiaoTu.XiaoTuCode.Powers;

/// <summary>手续费: gain Amount gold for each die rerolled.</summary>
public sealed class HandlingFeePower : XiaoTuPower, IDiceListener
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public async Task OnRerolls(PlayerChoiceContext ctx, int count)
    {
        Flash();
        await PlayerCmd.GainGold(Amount * count, Owner.Player!);
    }
}
