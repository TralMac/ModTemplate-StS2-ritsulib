using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Cards;
using XiaoTu.XiaoTuCode.Dice;

namespace XiaoTu.XiaoTuCode.Relics;

public sealed class MembershipCard : XiaoTuRelic, IDiceListener
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    private int _counter;

    public override bool ShowCounter => true;

    public override int DisplayAmount => _counter;

    public async Task OnDiceGained(PlayerChoiceContext ctx, int count)
    {
        _counter += count;
        var energy = _counter / 5;
        _counter %= 5;
        if (energy > 0)
        {
            Flash();
            await PlayerCmd.GainEnergy(energy, Owner);
        }
    }
}
