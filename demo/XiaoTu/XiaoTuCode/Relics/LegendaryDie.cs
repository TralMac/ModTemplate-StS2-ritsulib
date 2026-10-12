using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Cards;
using XiaoTu.XiaoTuCode.Dice;

namespace XiaoTu.XiaoTuCode.Relics;

public sealed class LegendaryDie : XiaoTuRelic
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override Task BeforeCombatStartLate()
    {
        DiceState.Of(Owner).Add(temporary: false, counts: false);
        return Task.CompletedTask;
    }
}
