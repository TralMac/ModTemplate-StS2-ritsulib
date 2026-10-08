using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using RitsuChar.RitsuCharCode.Character;
using STS2RitsuLib.Interop.AutoRegistration;

namespace RitsuChar.RitsuCharCode.Relics;

//Gives this relic to the character at the start of a run.
[RegisterCharacterStarterRelic(typeof(RitsuCharCharacter))]
public sealed class ExampleStarterRelic : RitsuCharRelic
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    //{Cards} in the localization refers to this CardsVar.
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner) return;

        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, player);
    }
}
