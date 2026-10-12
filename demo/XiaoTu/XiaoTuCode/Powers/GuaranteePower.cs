using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using XiaoTu.XiaoTuCode.Dice;

namespace XiaoTu.XiaoTuCode.Powers;

public sealed class GuaranteePower : XiaoTuPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
    {
        if (dealer != Owner || Owner.Player == null || !props.IsPoweredAttack()) return;
        var dice = DiceState.Of(Owner.Player);
        var floor = dice.Lowest;
        var missing = floor - result.UnblockedDamage;
        Flash();
        if (missing > 0 && !target.IsDead)
            await CreatureCmd.Damage(choiceContext, target, missing, ValueProp.Unblockable | ValueProp.Unpowered, Owner);
        //Upgraded (Amount 2): every attack also rerolls the lowest die, whether or not the floor was needed.
        if (Amount >= 2) dice.RerollLowest();
        await DiceDispatcher.Dispatch(choiceContext, Owner.Player);
    }
}
