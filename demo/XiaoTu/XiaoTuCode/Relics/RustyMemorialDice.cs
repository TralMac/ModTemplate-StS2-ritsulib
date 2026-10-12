using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Rewards;
using XiaoTu.XiaoTuCode.Potions;
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
        foreach (var def in STS2RitsuLib.Keywords.ModKeywordRegistry.GetDefinitionsSnapshot().Where(d => d.ModId == MainFile.ModId))
            MainFile.Logger.Info($"[Keywords] {def.Id} title={def.TitleTable}/{def.TitleKey} desc={def.DescriptionTable}/{def.DescriptionKey} card={def.CardKeywordValue}");
        var dice = DiceState.Of(Owner);
        dice.Clear();
        DiceHud.Bind(Owner);
        dice.Add(temporary: false, counts: false);
        return Task.CompletedTask;
    }

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner) return Task.CompletedTask;

        var dice = DiceState.Of(player);
        dice.BeginTurn();
        DiceHud.Bind(player);
        MainFile.Logger.Info($"[dice] turn start: {dice.Describe()} gold={player.Gold}");
        return DebugGrantCards(choiceContext);
    }

    public override Task AfterPlayerTurnStartLate(PlayerChoiceContext choiceContext, Player player) =>
        player == Owner ? Dispatch(choiceContext) : Task.CompletedTask;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner) return;
        MainFile.Logger.Info($"[dice] after {cardPlay.Card.GetType().Name}: {DiceState.Of(Owner).Describe()} gold={Owner.Gold}");
        await Dispatch(choiceContext);
    }

    /// <summary>Rebate (返利): refund part of the gold lost to card effects this combat.</summary>
    public override async Task AfterCombatVictory(CombatRoom room)
    {
        var rebate = Owner.Creature.GetPower<RebatePower>();
        var lost = DiceState.Of(Owner).GoldLostThisCombat;
        if (rebate == null || lost <= 0) return;
        await PlayerCmd.GainGold(lost * rebate.Amount / 100m, Owner);
    }

    /// <summary>
    /// Test harness: if mods/XiaoTu/debug_cards.txt exists, each turn start adds up to 6 of the listed card class names
    /// to the hand and tops up energy, so every card can be play-tested without deck building.
    /// </summary>
    private async Task DebugGrantCards(PlayerChoiceContext choiceContext)
    {
        var auditFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "debug_audit.txt");
        if (File.Exists(auditFile))
        {
            File.Delete(auditFile);
            var combatState = Owner.Creature.CombatState!;
            foreach (var canon in ModelDb.AllCards.Where(c => c.GetType().Namespace == "XiaoTu.XiaoTuCode.Cards").OrderBy(c => c.GetType().Name))
            {
                var card = combatState.CreateCard(canon, Owner);
                var up = combatState.CreateCard(canon, Owner);
                if (up.IsUpgradable) CardCmd.Upgrade(up);
                MainFile.Logger.Info($"[audit] {canon.GetType().Name}|{card.GetDescriptionForPile(PileType.Hand)}|{up.GetDescriptionForPile(PileType.Hand)}|{up.IsUpgraded}");
            }
        }

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
            var upgrade = name.EndsWith("+");
            var cardName = upgrade ? name[..^1] : name;
            var canon = ModelDb.AllCards.FirstOrDefault(c => c.GetType().Name == cardName);
            if (canon == null) { MainFile.Logger.Warn($"[debug] unknown card {name}"); continue; }
            var card = combat.CreateCard(canon, Owner);
            if (upgrade && card.IsUpgradable) CardCmd.Upgrade(card);
            await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner);
        }
    }

    private Task Dispatch(PlayerChoiceContext ctx) => DiceDispatcher.Dispatch(ctx, Owner);

    /// <summary>断舍离 / 崩老头: extra card and gold rewards owed after this combat.</summary>
    public override Task BeforeCombatRewardOffered(RewardsSet rewards, CombatRoom room)
    {
        var dice = DiceState.Of(Owner);
        for (var i = 0; i < dice.ExtraCardRewards; i++) rewards.Rewards.Add(new CardReward(CardCreationOptions.ForRoom(Owner, room.RoomType), 3, Owner));
        var gold = rewards.Rewards.OfType<GoldReward>().FirstOrDefault();
        for (var i = 0; i < dice.ExtraGoldRewards && gold != null; i++) rewards.Rewards.Add(new GoldReward(gold.Amount, Owner));
        if (dice.ChampagneOwed) rewards.Rewards.Add(new PotionReward(ModelDb.Potion<Champagne>().ToMutable(), Owner));
        dice.ChampagneOwed = false;
        dice.ExtraCardRewards = dice.ExtraGoldRewards = 0;
        return Task.CompletedTask;
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

