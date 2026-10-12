using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Cards;
using XiaoTu.XiaoTuCode.Dice;

namespace XiaoTu.XiaoTuCode.Relics;

public sealed class TaintedChurchCoin : XiaoTuRelic
{
    public override RelicRarity Rarity => RelicRarity.Common;

    public override bool IsAllowedInShops => false;

    public override async Task AfterObtained()
    {
        await PlayerCmd.GainGold(500, Owner);
        await CardPileCmd.AddCurseToDeck<Debt>(Owner);
    }
}
