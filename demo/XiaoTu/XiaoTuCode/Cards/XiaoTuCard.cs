using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using XiaoTu.XiaoTuCode.Character;
using XiaoTu.XiaoTuCode.Dice;
using XiaoTu.XiaoTuCode.Extensions;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace XiaoTu.XiaoTuCode.Cards;

/// <summary>
/// Base class for your character's cards.
/// Inherit = true makes every concrete subclass register itself into XiaoTuCardPool automatically.
/// A subclass can declare its own [RegisterCard(typeof(OtherPool))] to go into a different pool instead.
///
/// RitsuLib gives each card the public entry XIAO_TU_CARD_{CLASS_NAME_IN_UPPER_SNAKE_CASE},
/// which is also its localization key in localization/{language}/cards.json.
/// </summary>
[RegisterCard(typeof(XiaoTuCardPool), Inherit = true)]
public abstract class XiaoTuCard(int cost, CardType type, CardRarity rarity, TargetType target) :
    ModCardTemplate(cost, type, rarity, target)
{
    //Loads XiaoTu/images/cards/{ClassName}.png. The vanilla portrait size is 250x190 (250x351 for ancient cards).
    //When the image does not exist, RitsuLib's built-in placeholder portrait is used.
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"cards/{GetType().Name}.png".ImagePathOr(null)
    );

    protected DiceState Dice => DiceState.Of(Owner);

    /// <summary>Whether the card has at least n dice to pledge; used by 质押 cards' IsPlayable.</summary>
    protected bool HasDice(int n) => Owner != null && DiceState.Of(Owner).Count >= n;

    protected Task Hit(PlayerChoiceContext ctx, CardPlay play, decimal damage, int hits = 1)
    {
        var attack = DamageCmd.Attack(damage).FromCard(this, play).WithHitCount(hits);
        return (TargetType == TargetType.AllEnemies
            ? attack.TargetingAllOpponents(CombatState!)
            : TargetType == TargetType.RandomEnemy
                ? attack.TargetingRandomOpponents(CombatState!)
                : attack.Targeting(play.Target!)).Execute(ctx);
    }

    protected Task Block(CardPlay play, decimal amount, int times = 1) => GainBlockAsync(play, amount, times);

    private async Task GainBlockAsync(CardPlay play, decimal amount, int times)
    {
        for (var i = 0; i < times; i++)
            await CreatureCmd.GainBlock(Owner.Creature, amount, ValueProp.Move, play);
    }

    protected Task Draw(PlayerChoiceContext ctx, int n) => CardPileCmd.Draw(ctx, n, Owner);

    protected Task LoseGold(int n)
    {
        GoldOps.Lose(Owner, n);
        return Task.CompletedTask;
    }
}
