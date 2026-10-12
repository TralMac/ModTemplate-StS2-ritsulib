using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace XiaoTu.XiaoTuCode.Potions;

/// <summary>香槟: stuns a non-boss enemy for a turn. Obtained from 自罚一杯 at the end of combat.</summary>
public sealed class Champagne : XiaoTuPotion
{
    public override PotionRarity Rarity => PotionRarity.Uncommon;

    public override PotionUsage Usage => PotionUsage.CombatOnly;

    public override TargetType TargetType => TargetType.AnyEnemy;

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        PotionModel.AssertValidForTargetedPotion(target);
        if (Owner.Creature.CombatState?.Encounter?.RoomType == RoomType.Boss) return;
        await CreatureCmd.Stun(target!);
    }
}
