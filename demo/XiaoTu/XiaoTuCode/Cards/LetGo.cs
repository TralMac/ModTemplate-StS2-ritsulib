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

public sealed class LetGo() : XiaoTuCard(1, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Amount", 4m)];

    protected override bool IsPlayable => Owner != null && DiceState.Of(Owner).Capacity > 0;

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        Dice.LoseSlots(DynamicVars["Amount"].IntValue);
        Dice.ExtraCardRewards++;
        await Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Amount"].UpgradeValueBy(-1m);
    }
}
