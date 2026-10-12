using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace XiaoTu.XiaoTuCode.Dice;

/// <summary>Hands the events collected by <see cref="DiceState"/> to every listening power and relic.</summary>
public static class DiceDispatcher
{
    private static bool _running;

    public static async Task Dispatch(PlayerChoiceContext ctx, Player player)
    {
        if (_running) return;
        _running = true;
        try
        {
            // Listeners may trigger further events (e.g. rerolls from a power), so loop until quiet.
            for (var guard = 0; guard < 20; guard++)
            {
                var dice = DiceState.Of(player);
                var rerolls = dice.TakeRerolls();
                var gains = dice.TakeGains();
                var lost = dice.TakeGoldLost();
                var bets = dice.TakeBets();
                var pledged = dice.TakePledgeEvents();
                if (rerolls + gains + lost + bets.Count + pledged == 0) return;
                var listeners = player.Creature.Powers.OfType<IDiceListener>()
                    .Concat(player.Relics.OfType<IDiceListener>()).ToList();
                foreach (var l in listeners)
                {
                    if (rerolls > 0) await l.OnRerolls(ctx, rerolls);
                    if (gains > 0) await l.OnDiceGained(ctx, gains);
                    if (lost > 0) await l.OnGoldLost(ctx, lost);
                    if (bets.Count > 0) await l.OnBets(ctx, bets);
                    if (pledged > 0) await l.OnPledged(ctx, pledged);
                }
            }
        }
        finally
        {
            _running = false;
        }
    }
}
