using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Entities.Players;

namespace XiaoTu.XiaoTuCode.Dice;

/// <summary>
/// Per-player dice for the current combat (see 骰子角色设计表 → 机制与规则).
/// Demo implementation: dice are plain ints, rolled with System.Random (not seed-synced with the run RNG).
/// </summary>
public sealed class DiceState
{
    public const int MaxDice = 5;

    private static readonly ConditionalWeakTable<Player, DiceState> States = new();
    private static readonly Random Rng = new();

    private readonly List<Die> _dice = [];

    public IReadOnlyList<Die> Dice => _dice;

    /// <summary>总骰点：所有骰子点数之和。</summary>
    public int Total => _dice.Sum(d => d.Value);

    public int Count => _dice.Count;

    public int EvenCount => _dice.Count(d => d.Value % 2 == 0);

    public int OddCount => _dice.Count(d => d.Value % 2 != 0);

    public static DiceState Of(Player player) => States.GetValue(player, _ => new DiceState());

    public static int Roll() => Rng.Next(1, 7);

    public void Clear() => _dice.Clear();

    /// <summary>Adds a die with a random value. Over the cap, a random temp die is replaced, else a random die.</summary>
    public void Add(bool temporary)
    {
        var die = new Die(Roll(), temporary);
        if (_dice.Count < MaxDice)
        {
            _dice.Add(die);
            return;
        }

        var temps = _dice.Select((d, i) => (d, i)).Where(x => x.d.Temporary).Select(x => x.i).ToList();
        var index = temps.Count > 0 ? temps[Rng.Next(temps.Count)] : Rng.Next(_dice.Count);
        _dice[index] = die;
    }

    /// <summary>Rerolls one die (random) and returns its new value; 0 when there are no dice.</summary>
    public int RerollRandom()
    {
        if (_dice.Count == 0) return 0;
        var i = Rng.Next(_dice.Count);
        _dice[i] = _dice[i] with { Value = Roll() };
        return _dice[i].Value;
    }

    public void RerollAll()
    {
        for (var i = 0; i < _dice.Count; i++) _dice[i] = _dice[i] with { Value = Roll() };
    }

    /// <summary>Temp dice vanish when the enemy turn ends, i.e. just before the next player turn starts.</summary>
    public void RemoveTemporary() => _dice.RemoveAll(d => d.Temporary);
}

public readonly record struct Die(int Value, bool Temporary);
