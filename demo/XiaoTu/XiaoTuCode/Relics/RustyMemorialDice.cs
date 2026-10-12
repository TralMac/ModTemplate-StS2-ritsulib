using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
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
        dice.Add(temporary: false);
        return Task.CompletedTask;
    }

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner) return Task.CompletedTask;

        var dice = DiceState.Of(player);
        dice.RemoveTemporary();
        dice.TickPledges();
        dice.RerollAll();
        return Task.CompletedTask;
    }

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer,
        CardModel? cardSource, CardPlay? cardPlay)
    {
        if (target != Owner.Creature || !props.IsPoweredAttack()) return 1m;
        return GoldOps.DebtMultiplier(Owner);
    }
}

