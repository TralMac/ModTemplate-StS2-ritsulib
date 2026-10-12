using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using XiaoTu.XiaoTuCode.Dice;

namespace XiaoTu.XiaoTuCode.Powers;

/// <summary>天选之人: at the start of each turn, your lowest die is rerolled once more (Amount times).</summary>
public sealed class ChosenOnePower : XiaoTuPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override Task AfterPlayerTurnStartLate(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player) return Task.CompletedTask;
        Flash();
        var dice = DiceState.Of(player);
        for (var i = 0; i < Amount; i++) dice.RerollLowest();
        return Task.CompletedTask;
    }
}
