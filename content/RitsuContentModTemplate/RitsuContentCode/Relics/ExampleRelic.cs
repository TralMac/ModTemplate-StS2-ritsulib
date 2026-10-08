using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace RitsuContent.RitsuContentCode.Relics;

//Adds this relic to the pool shared by all characters.
[RegisterRelic(typeof(SharedRelicPool))]
public sealed class ExampleRelic : RitsuContentRelic
{
    public override RelicRarity Rarity => RelicRarity.Common;

    //{Block} in the localization refers to this BlockVar.
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(2, ValueProp.Unpowered)];

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner) return;

        await CreatureCmd.GainBlock(player.Creature, DynamicVars.Block, null);
    }
}
