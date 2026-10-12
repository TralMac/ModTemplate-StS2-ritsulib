using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Rooms;
using XiaoTu.XiaoTuCode.Powers;
using System.Reflection;
using XiaoTu.XiaoTuCode.Character;
using XiaoTu.XiaoTuCode.Dice;
using STS2RitsuLib.Interop.AutoRegistration;

namespace XiaoTu.XiaoTuCode.Relics;

/// <summary>
/// 生锈的纪念骰子. Starts each combat with 1 die; rerolls every die at the start of each turn (returning pledged dice first).
/// Gold may go negative (floor -1000, see <see cref="GoldOps"/>); each gold below 0 adds 0.5% to attack damage taken.
/// </summary>
[RegisterCharacterStarterRelic(typeof(XiaoTuCharacter))]
public sealed class RustyMemorialDice : XiaoTuRelic
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    public override Task BeforeCombatStart()
    {
        var dice = DiceState.Of(Owner);
        dice.Clear();
        dice.Add(temporary: false, counts: false);
        return Task.CompletedTask;
    }

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner) return Task.CompletedTask;

        var dice = DiceState.Of(player);
        dice.RemoveTemporary();
        dice.TickPledges();
        dice.RerollAll(counts: false);
        MainFile.Logger.Info($"[dice] turn start: {dice.Describe()} gold={player.Gold}");
        return DebugGrantCards(choiceContext);
    }

    public override Task AfterPlayerTurnStartLate(PlayerChoiceContext choiceContext, Player player) =>
        player == Owner ? Dispatch(choiceContext) : Task.CompletedTask;

    /// <summary>Rebate (返利): refund part of the gold lost to card effects this combat.</summary>
    public override async Task AfterCombatVictory(CombatRoom room)
    {
        var rebate = Owner.Creature.GetPower<RebatePower>();
        var lost = DiceState.Of(Owner).GoldLostThisCombat;
        if (rebate == null || lost <= 0) return;
        await PlayerCmd.GainGold(lost * rebate.Amount / 100m, Owner);
    }

    /// <summary>Hands collected reroll / gain / gold-loss events to every listening power and relic.</summary>
    private async Task Dispatch(PlayerChoiceContext ctx)
    {
        var dice = DiceState.Of(Owner);
        var rerolls = dice.TakeRerolls();
        var gains = dice.TakeGains();
        var lost = dice.TakeGoldLost();
        if (rerolls + gains + lost == 0) return;
        var listeners = Owner.Creature.Powers.OfType<IDiceListener>().Concat(Owner.Relics.OfType<IDiceListener>()).ToList();
        foreach (var l in listeners)
        {
            if (rerolls > 0) await l.OnRerolls(ctx, rerolls);
            if (gains > 0) await l.OnDiceGained(ctx, gains);
            if (lost > 0) await l.OnGoldLost(ctx, lost);
        }
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner) return;
        MainFile.Logger.Info($"[dice] after {cardPlay.Card.GetType().Name}: {DiceState.Of(Owner).Describe()} gold={Owner.Gold}");
        await Dispatch(choiceContext);
    }

    /// <summary>
    /// Test harness: if mods/XiaoTu/debug_cards.txt exists, each turn start adds up to 6 of the listed card class names
    /// to the hand and tops up energy, so every card can be play-tested without deck building.
    /// </summary>
    private async Task DebugGrantCards(PlayerChoiceContext choiceContext)
    {
        var file = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "debug_cards.txt");
        if (!File.Exists(file)) return;
        var names = File.ReadAllLines(file).Select(l => l.Trim()).Where(l => l.Length > 0).ToList();
        var batch = names.Take(6).ToList();
        File.WriteAllLines(file, names.Skip(6));
        if (batch.Count == 0) return;
        var combat = Owner.Creature.CombatState!;
        await PlayerCmd.GainEnergy(10, Owner);
        foreach (var name in batch)
        {
            var canon = ModelDb.AllCards.FirstOrDefault(c => c.GetType().Name == name);
            if (canon == null) { MainFile.Logger.Warn($"[debug] unknown card {name}"); continue; }
            var card = combat.CreateCard(canon, Owner);
            await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner);
        }
    }

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer,
        CardModel? cardSource, CardPlay? cardPlay)
    {
        if (target != Owner.Creature || !props.IsPoweredAttack()) return 1m;
        var mult = GoldOps.DebtMultiplier(Owner);
        if (mult > 1m) MainFile.Logger.Info($"[debt] x{mult} on {amount} damage (gold={Owner.Gold})");
        return mult;
    }
}

