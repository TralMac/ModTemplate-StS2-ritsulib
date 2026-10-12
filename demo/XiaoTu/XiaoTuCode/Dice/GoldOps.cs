using MegaCrit.Sts2.Core.Entities.Players;

namespace XiaoTu.XiaoTuCode.Dice;

/// <summary>Gold that may go negative (floor -1000), provided by the 生锈的纪念骰子 design.</summary>
public static class GoldOps
{
    public const int Floor = -1000;

    /// <summary>Loses gold down to the -1000 floor; returns the amount actually lost.</summary>
    public static int Lose(Player player, int amount)
    {
        var before = player.Gold;
        player.Gold = Math.Max(Floor, before - amount);
        var lost = before - player.Gold;
        DiceState.Of(player).NoteGoldLost(lost);
        return lost;
    }

    /// <summary>Damage multiplier from debt: +0.5% per gold below 0 (rounded to nearest at use site).</summary>
    public static decimal DebtMultiplier(Player player) => player.Gold >= 0 ? 1m : 1m + -player.Gold * 0.005m;
}
