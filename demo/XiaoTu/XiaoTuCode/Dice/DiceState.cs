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

    public void Clear()
    {
        _dice.Clear();
        _pledges.Clear();
    }

    /// <summary>Adds a die with a random value. Over the cap, a random temp die is replaced, else a random die.</summary>
    public int Add(bool temporary)
    {
        var die = new Die(Roll(), temporary);
        if (_dice.Count < MaxDice)
        {
            _dice.Add(die);
            return die.Value;
        }

        var temps = _dice.Select((d, i) => (d, i)).Where(x => x.d.Temporary).Select(x => x.i).ToList();
        var index = temps.Count > 0 ? temps[Rng.Next(temps.Count)] : Rng.Next(_dice.Count);
        _dice[index] = die;
        return die.Value;
    }

    /// <summary>Rerolls the lowest die; success when the new value is strictly greater.</summary>
    public bool BetHighLowest()
    {
        if (_dice.Count == 0) return false;
        var i = _dice.Select((d, k) => (d, k)).MinBy(x => x.d.Value).k;
        var old = _dice[i].Value;
        _dice[i] = _dice[i] with { Value = Roll() };
        return _dice[i].Value > old;
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

    /// <summary>Rerolls one random die; success when the new value is strictly greater (押大). False when no dice.</summary>
    public bool BetHigh()
    {
        if (_dice.Count == 0) return false;
        var i = Rng.Next(_dice.Count);
        var old = _dice[i].Value;
        _dice[i] = _dice[i] with { Value = Roll() };
        return _dice[i].Value > old;
    }

    /// <summary>Rerolls every die matching the predicate; returns how many were rerolled.</summary>
    public int RerollWhere(Func<int, bool> pred)
    {
        var n = 0;
        for (var i = 0; i < _dice.Count; i++)
        {
            if (!pred(_dice[i].Value)) continue;
            _dice[i] = _dice[i] with { Value = Roll() };
            n++;
        }
        return n;
    }

    /// <summary>Adds delta to every die matching the predicate, clamped to 1..6.</summary>
    public void Shift(int delta, Func<int, bool>? pred = null)
    {
        for (var i = 0; i < _dice.Count; i++)
            if (pred?.Invoke(_dice[i].Value) ?? true)
                _dice[i] = _dice[i] with { Value = Math.Clamp(_dice[i].Value + delta, 1, 6) };
    }

    /// <summary>Adds delta to one random die, clamped to 1..6.</summary>
    public void ShiftRandom(int delta)
    {
        if (_dice.Count == 0) return;
        var i = Rng.Next(_dice.Count);
        _dice[i] = _dice[i] with { Value = Math.Clamp(_dice[i].Value + delta, 1, 6) };
    }

    /// <summary>Rerolls one random die whose value is not 6 (666).</summary>
    public void RerollRandomNot6()
    {
        var idx = _dice.Select((d, i) => (d, i)).Where(x => x.d.Value != 6).Select(x => x.i).ToList();
        if (idx.Count == 0) return;
        var i = idx[Rng.Next(idx.Count)];
        _dice[i] = _dice[i] with { Value = Roll() };
    }

    /// <summary>The lowest die becomes equal to the highest (跟风).</summary>
    public void MatchLowestToHighest()
    {
        if (_dice.Count == 0) return;
        var lo = _dice.Select((d, i) => (d, i)).MinBy(x => x.d.Value).i;
        _dice[lo] = _dice[lo] with { Value = _dice.Max(d => d.Value) };
    }

    /// <summary>Mirrors every die: 1↔6, 2↔5, 3↔4.</summary>
    public void Invert()
    {
        for (var i = 0; i < _dice.Count; i++) _dice[i] = _dice[i] with { Value = 7 - _dice[i].Value };
    }

    /// <summary>Number of dice sharing a value with at least one other die (开幕雷击).</summary>
    public int PairedCount => _dice.GroupBy(d => d.Value).Where(g => g.Count() > 1).Sum(g => g.Count());

    public bool HasPair => PairedCount > 0;

    public int Lowest => _dice.Count == 0 ? 0 : _dice.Min(d => d.Value);

    /// <summary>True when the dice contain a run of at least three consecutive values (顺子).</summary>
    public bool HasStraight
    {
        get
        {
            var set = _dice.Select(d => d.Value).ToHashSet();
            return Enumerable.Range(1, 4).Any(s => set.Contains(s) && set.Contains(s + 1) && set.Contains(s + 2));
        }
    }

    // ---- 质押 (pledge): a die leaves for X turns, comes back (random value) at the start of the Xth turn.
    private readonly List<int> _pledges = [];

    public int PledgedCount => _pledges.Count;

    /// <summary>Pledges up to count random dice for turns turns; temp dice are simply lost.</summary>
    public void Pledge(int count, int turns)
    {
        for (var k = 0; k < count && _dice.Count > 0; k++)
        {
            var i = Rng.Next(_dice.Count);
            var die = _dice[i];
            _dice.RemoveAt(i);
            if (!die.Temporary) _pledges.Add(turns);
        }
    }

    /// <summary>Called at the start of each player turn: counts down and returns due dice (no effect when full).</summary>
    public int TickPledges()
    {
        var returned = 0;
        for (var i = _pledges.Count - 1; i >= 0; i--)
        {
            if (--_pledges[i] > 0) continue;
            _pledges.RemoveAt(i);
            if (_dice.Count >= MaxDice) continue;
            _dice.Add(new Die(Roll(), false));
            returned++;
        }
        return returned;
    }

    public void ClearPledges() => _pledges.Clear();

    /// <summary>Temp dice vanish when the enemy turn ends, i.e. just before the next player turn starts.</summary>
    public void RemoveTemporary() => _dice.RemoveAll(d => d.Temporary);
}

public readonly record struct Die(int Value, bool Temporary);
