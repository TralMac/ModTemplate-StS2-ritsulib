using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace XiaoTu.XiaoTuCode.Dice;

/// <summary>
/// Implemented by powers and relics that react to dice events. Events are collected by <see cref="DiceState"/> and
/// dispatched by the starter relic after each card play and at turn start.
/// </summary>
public interface IDiceListener
{
    /// <summary>Dice rerolled by effects (each die counts once; the automatic turn-start roll does not).</summary>
    Task OnRerolls(PlayerChoiceContext ctx, int count) => Task.CompletedTask;

    /// <summary>Dice gained by effects (the automatic turn-start roll does not count).</summary>
    Task OnDiceGained(PlayerChoiceContext ctx, int count) => Task.CompletedTask;

    /// <summary>Gold lost to card effects.</summary>
    Task OnGoldLost(PlayerChoiceContext ctx, int amount) => Task.CompletedTask;
}
