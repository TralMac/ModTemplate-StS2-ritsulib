using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Entities.Players;

namespace XiaoTu.XiaoTuCode.Dice;

/// <summary>
/// Per-player dice for the current combat (see 骰子角色设计表 → 机制与规则).
/// Dice are plain ints rolled with System.Random (not seed-synced with the run RNG).
/// </summary>
public sealed class DiceState
{
    public const int MaxDice = 5;

    private static readonly ConditionalWeakTable<Player, DiceState> States = new();
    private static readonly Random Rng = new();

    private readonly List<Die> _dice = [];
    private readonly List<(int Turns, int Copy)> _pledges = [];
    private readonly List<bool> _bets = [];

    private int _rerolls;
    private int _gains;
    private int _goldLost;
    private int _pledgeEvents;

    public IReadOnlyList<Die> Dice => _dice;

    /// <summary>Dice slots for this combat (骰子位); 断舍离 / 弱女子 lower it.</summary>
    public int Capacity { get; private set; } = MaxDice;

    /// <summary>赌圣形态: dice never show a value below this.</summary>
    public int Floor { get; set; } = 1;

    /// <summary>从头再来: a reroll always lands higher than before (a 6 becomes 1).</summary>
    public bool ForwardOnly { get; set; }

    /// <summary>这次不算: a die that rolls 1 is rerolled automatically.</summary>
    public bool RerollOnOne { get; set; }

    /// <summary>预借骰子: temp dice gained now survive one extra turn and are lowered by <see cref="BorrowPenalty"/>.</summary>
    public bool BorrowActive { get; set; }

    public int BorrowPenalty { get; set; } = 2;

    /// <summary>连本带利: pledged dice come back showing this value (0 = random).</summary>
    public int PledgeReturnValue { get; set; }

    /// <summary>Gold lost to card effects this combat (for 返利).</summary>
    public int GoldLostThisCombat { get; private set; }

    /// <summary>Dice rerolled by effects this turn (杯里乾坤).</summary>
    public int RerollsThisTurn { get; private set; }

    /// <summary>Times a die was pledged or gained this combat, including the automatic turn-start rolls (利滚利).</summary>
    public int GainOrPledgeCount { get; private set; }

    /// <summary>Extra card / gold rewards owed at combat end (断舍离, 崩老头).</summary>
    public int ExtraCardRewards { get; set; }

    public int ExtraGoldRewards { get; set; }

    /// <summary>总骰点：所有骰子点数之和。</summary>
    public int Total => _dice.Sum(d => d.Value);

    public int Count => _dice.Count;

    /// <summary>薛定谔的骰子: every die counts as both odd and even.</summary>
    public bool Schrodinger { get; set; }

    /// <summary>自罚一杯: a Champagne potion is owed at the end of this combat.</summary>
    public bool ChampagneOwed { get; set; }

    public bool IsOdd(int v) => Schrodinger || v % 2 != 0;

    public bool IsEven(int v) => Schrodinger || v % 2 == 0;

    public int EvenCount => _dice.Count(d => IsEven(d.Value));

    public int OddCount => _dice.Count(d => IsOdd(d.Value));

    /// <summary>True when all dice share parity (清一色); always true with 薛定谔的骰子.</summary>
    public bool AllSameParity => _dice.Count > 0 && (Schrodinger || EvenCount == 0 || OddCount == 0);

    public int PledgedCount => _pledges.Count;

    public int Lowest => _dice.Count == 0 ? 0 : _dice.Min(d => d.Value);

    /// <summary>Number of dice sharing a value with at least one other die (开幕雷击).</summary>
    public int PairedCount => _dice.GroupBy(d => d.Value).Where(g => g.Count() > 1).Sum(g => g.Count());

    public bool HasPair => PairedCount > 0;

    public bool HasTriple => _dice.GroupBy(d => d.Value).Any(g => g.Count() >= 3);

    /// <summary>True when the dice contain a run of at least three consecutive values (顺子).</summary>
    public bool HasStraight
    {
        get
        {
            var set = _dice.Select(d => d.Value).ToHashSet();
            return Enumerable.Range(1, 4).Any(s => set.Contains(s) && set.Contains(s + 1) && set.Contains(s + 2));
        }
    }

    public static DiceState Of(Player player)
    {
        var state = States.GetValue(player, _ => new DiceState());
        state.ApplyFloor();
        return state;
    }

    public static int Roll() => Rng.Next(1, 7);

    public string Describe() =>
        $"[{string.Join(",", _dice.Select(d => d.Value + (d.Temporary ? "t" : "") + (d.Fixed ? "f" : "")))}] total={Total} pledged={PledgedCount} cap={Capacity}";

    // ---- event counters, collected by DiceDispatcher ----
    public int TakeRerolls() { var n = _rerolls; _rerolls = 0; return n; }

    public int TakeGains() { var n = _gains; _gains = 0; return n; }

    public int TakePledgeEvents() { var n = _pledgeEvents; _pledgeEvents = 0; return n; }

    public int TakeGoldLost() { var n = _goldLost; _goldLost = 0; return n; }

    public List<bool> TakeBets()
    {
        var l = _bets.ToList();
        _bets.Clear();
        return l;
    }

    public void NoteGoldLost(int n)
    {
        _goldLost += n;
        GoldLostThisCombat += n;
    }

    public void Clear()
    {
        _dice.Clear();
        _pledges.Clear();
        _bets.Clear();
        Capacity = MaxDice;
        Floor = 1;
        ForwardOnly = RerollOnOne = BorrowActive = Schrodinger = ChampagneOwed = false;
        BorrowPenalty = 2;
        PledgeReturnValue = 0;
        _pledgeEvents = 0;
        _rerolls = _gains = _goldLost = 0;
        GoldLostThisCombat = RerollsThisTurn = GainOrPledgeCount = 0;
        ExtraCardRewards = ExtraGoldRewards = 0;
    }

    private void ApplyFloor()
    {
        for (var i = 0; i < _dice.Count; i++)
            if (!_dice[i].Fixed && _dice[i].Value < Floor)
                _dice[i] = _dice[i] with { Value = Floor };
    }

    // ---- rolling ----
    private int InitialRoll()
    {
        var v = Roll();
        while (RerollOnOne && v == 1)
        {
            _rerolls++;
            RerollsThisTurn++;
            v = Roll();
        }
        return Math.Max(v, Floor);
    }

    private int NextValue(int old)
    {
        if (ForwardOnly)
        {
            // Deterministic direction: never loops, and a 6 turning into 1 is the whole point of the effect.
            return Math.Max(old >= 6 ? 1 : Rng.Next(old + 1, 7), Floor);
        }
        var v = Roll();
        for (var guard = 0; RerollOnOne && v == 1 && guard < 50; guard++)
        {
            _rerolls++;
            RerollsThisTurn++;
            v = Roll();
        }
        return Math.Max(v, Floor);
    }

    /// <summary>Rerolls die i (counts as a reroll); returns whether the value went up.</summary>
    private bool Reroll(int i)
    {
        _rerolls++;
        RerollsThisTurn++;
        if (_dice[i].Fixed) return true;
        var old = _dice[i].Value;
        var v = NextValue(old);
        _dice[i] = _dice[i] with { Value = v };
        return v > old;
    }

    private void Set(int i, int v)
    {
        if (_dice[i].Fixed) return;
        _dice[i] = _dice[i] with { Value = Math.Clamp(v, Math.Max(1, Floor), 6) };
    }

    /// <summary>Adds a die with a random value. Over the cap, a random temp die is replaced, else a random die.</summary>
    public int Add(bool temporary, bool counts = true)
    {
        if (Capacity == 0) return 0;
        if (counts)
        {
            _gains++;
            GainOrPledgeCount++;
        }

        var value = InitialRoll();
        var extra = 0;
        if (temporary && BorrowActive)
        {
            value = Math.Max(1, value - BorrowPenalty);
            extra = 1;
        }

        var die = new Die(value, temporary, extra);
        if (_dice.Count < Capacity)
        {
            _dice.Add(die);
            return die.Value;
        }

        var temps = _dice.Select((d, i) => (d, i)).Where(x => x.d.Temporary && !x.d.Fixed).Select(x => x.i).ToList();
        var pool = temps.Count > 0 ? temps : _dice.Select((d, i) => (d, i)).Where(x => !x.d.Fixed).Select(x => x.i).ToList();
        if (pool.Count == 0) return 0;
        _dice[pool[Rng.Next(pool.Count)]] = die;
        return die.Value;
    }

    /// <summary>奇迹骰子: a die locked at 6, immune to every value-changing effect.</summary>
    public void AddFixed(int value)
    {
        if (Capacity == 0) return;
        _gains++;
        GainOrPledgeCount++;
        var die = new Die(value, false, 0, true);
        if (_dice.Count < Capacity) _dice.Add(die);
        else
        {
            var pool = _dice.Select((d, i) => (d, i)).Where(x => !x.d.Fixed).Select(x => x.i).ToList();
            if (pool.Count > 0) _dice[pool[Rng.Next(pool.Count)]] = die;
        }
    }

    // ---- rerolls ----
    /// <summary>Rerolls one die (random) and returns its new value; 0 when there are no dice.</summary>
    public int RerollRandom()
    {
        if (_dice.Count == 0) return 0;
        var i = Rng.Next(_dice.Count);
        Reroll(i);
        return _dice[i].Value;
    }

    /// <summary>Rerolls one random die; success when the new value is strictly greater (押大). False when no dice.</summary>
    public bool BetHigh()
    {
        if (_dice.Count == 0) return false;
        var ok = Reroll(Rng.Next(_dice.Count));
        _bets.Add(ok);
        return ok;
    }

    /// <summary>Rerolls the lowest die; success when the new value is strictly greater.</summary>
    public bool BetHighLowest()
    {
        if (_dice.Count == 0) return false;
        var ok = Reroll(_dice.Select((d, k) => (d, k)).MinBy(x => x.d.Value).k);
        _bets.Add(ok);
        return ok;
    }

    /// <summary>Rerolls every die; counts is false for the automatic turn-start roll.</summary>
    public void RerollAll(bool counts = true)
    {
        for (var i = 0; i < _dice.Count; i++)
        {
            if (counts) Reroll(i);
            else if (!_dice[i].Fixed) _dice[i] = _dice[i] with { Value = InitialRoll() };
        }
    }

    /// <summary>Rerolls every die matching the predicate; returns how many were rerolled.</summary>
    public int RerollWhere(Func<int, bool> pred)
    {
        var n = 0;
        for (var i = 0; i < _dice.Count; i++)
        {
            if (!pred(_dice[i].Value)) continue;
            Reroll(i);
            n++;
        }
        return n;
    }

    public void RerollLowest()
    {
        if (_dice.Count == 0) return;
        Reroll(_dice.Select((d, k) => (d, k)).MinBy(x => x.d.Value).k);
    }

    /// <summary>Rerolls one random die whose value is not 6 (666).</summary>
    public void RerollRandomNot6()
    {
        var idx = _dice.Select((d, i) => (d, i)).Where(x => x.d.Value != 6).Select(x => x.i).ToList();
        if (idx.Count == 0) return;
        Reroll(idx[Rng.Next(idx.Count)]);
    }

    // ---- value edits ----
    /// <summary>Adds delta to every die matching the predicate, clamped to 1..6.</summary>
    public void Shift(int delta, Func<int, bool>? pred = null)
    {
        for (var i = 0; i < _dice.Count; i++)
            if (pred?.Invoke(_dice[i].Value) ?? true)
                Set(i, _dice[i].Value + delta);
    }

    public void ShiftRandom(int delta)
    {
        if (_dice.Count == 0) return;
        var i = Rng.Next(_dice.Count);
        Set(i, _dice[i].Value + delta);
    }

    /// <summary>Mirrors every die: 1↔6, 2↔5, 3↔4.</summary>
    public void Invert()
    {
        for (var i = 0; i < _dice.Count; i++) Set(i, 7 - _dice[i].Value);
    }

    /// <summary>The lowest die becomes equal to the highest (跟风).</summary>
    public void MatchLowestToHighest()
    {
        if (_dice.Count == 0) return;
        var lo = _dice.Select((d, i) => (d, i)).MinBy(x => x.d.Value).i;
        Set(lo, _dice.Max(d => d.Value));
    }

    // ---- 骰子位 ----
    /// <summary>Removes dice slots; surplus dice are dropped at random, temp dice first.</summary>
    public void LoseSlots(int n)
    {
        Capacity = Math.Max(0, Capacity - n);
        while (_dice.Count > Capacity)
        {
            var temps = _dice.Select((d, i) => (d, i)).Where(x => x.d.Temporary).Select(x => x.i).ToList();
            var idx = temps.Count > 0 ? temps[Rng.Next(temps.Count)] : Rng.Next(_dice.Count);
            _dice.RemoveAt(idx);
        }
    }

    // ---- 质押 (pledge): a die leaves for X turns, comes back (random value) at the start of the Xth turn. ----
    /// <summary>
    /// Pledges up to count random dice for turns turns; temp dice are simply lost. With duplicate, real dice are
    /// pledged first and the die comes back together with a same-valued copy (骰子戏法).
    /// </summary>
    public void Pledge(int count, int turns, bool duplicate = false)
    {
        for (var k = 0; k < count && _dice.Count > 0; k++)
        {
            var pool = _dice.Select((d, i) => (d, i)).Where(x => !x.d.Fixed).Select(x => x.i).ToList();
            if (duplicate)
            {
                var real = pool.Where(i => !_dice[i].Temporary).ToList();
                if (real.Count > 0) pool = real;
            }
            if (pool.Count == 0) return;
            var idx = pool[Rng.Next(pool.Count)];
            var die = _dice[idx];
            _dice.RemoveAt(idx);
            GainOrPledgeCount++;
            _pledgeEvents++;
            if (!die.Temporary) _pledges.Add((turns, duplicate ? die.Value : 0));
        }
    }

    /// <summary>Called at the start of each player turn: counts down and returns due dice (no effect when full).</summary>
    public int TickPledges()
    {
        var returned = 0;
        for (var i = _pledges.Count - 1; i >= 0; i--)
        {
            var (turns, copy) = _pledges[i];
            if (turns - 1 > 0)
            {
                _pledges[i] = (turns - 1, copy);
                continue;
            }
            _pledges.RemoveAt(i);
            if (_dice.Count >= Capacity) continue;
            _dice.Add(new Die(PledgeReturnValue > 0 ? PledgeReturnValue : InitialRoll(), false));
            returned++;
            if (copy > 0 && _dice.Count < Capacity) _dice.Add(new Die(Math.Max(copy, Floor), false));
        }
        return returned;
    }

    /// <summary>
    /// Start of a player turn: temp dice vanish (borrowed ones one turn later), pledges tick, every die rolls
    /// (this automatic roll is not a reroll but does count toward 利滚利).
    /// </summary>
    public void BeginTurn()
    {
        _dice.RemoveAll(d => d.Temporary && d.Extra == 0);
        for (var i = 0; i < _dice.Count; i++)
            if (_dice[i].Temporary && _dice[i].Extra > 0)
                _dice[i] = _dice[i] with { Extra = _dice[i].Extra - 1 };
        BorrowActive = false;
        RerollsThisTurn = 0;
        TickPledges();
        RerollAll(counts: false);
        GainOrPledgeCount += _dice.Count;
    }
}

public readonly record struct Die(int Value, bool Temporary, int Extra = 0, bool Fixed = false);
