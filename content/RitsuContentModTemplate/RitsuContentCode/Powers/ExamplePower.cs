using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace RitsuContent.RitsuContentCode.Powers;

public sealed class ExamplePower : RitsuContentPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    //Whenever the owner draws a card, gain Block equal to this power's amount.
    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card.Owner != Owner.Player) return;

        await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null);
    }
}
