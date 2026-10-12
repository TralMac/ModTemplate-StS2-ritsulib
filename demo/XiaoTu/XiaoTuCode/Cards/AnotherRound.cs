using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using XiaoTu.XiaoTuCode.Dice;
using XiaoTu.XiaoTuCode.Powers;

namespace XiaoTu.XiaoTuCode.Cards;

public sealed class AnotherRound() : XiaoTuCard(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        Dice.RerollAll();
        var piles = new[] { PileType.Hand, PileType.Discard };
        var moving = piles.SelectMany(p => p.GetPile(Owner).Cards).Where(c => c != this).ToList();
        if (moving.Count > 0) await CardPileCmd.Add(moving, PileType.Draw);
        var statuses = PileType.Draw.GetPile(Owner).Cards.Where(c => c.Type == CardType.Status).ToList();
        foreach (var c in statuses) await CardCmd.Exhaust(ctx, c);
        await CardPileCmd.Shuffle(ctx, Owner);
        await Draw(ctx, 5);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
