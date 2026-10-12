using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using XiaoTu.XiaoTuCode.Character;
using XiaoTu.XiaoTuCode.Dice;
using STS2RitsuLib.Interop.AutoRegistration;

namespace XiaoTu.XiaoTuCode.Relics;

/// <summary>
/// 生锈的纪念骰子. Demo scope: starts each combat with 1 die and rerolls every die at the start of each turn.
/// The negative-gold / debt-damage part of the design is not implemented yet.
/// </summary>
[RegisterCharacterStarterRelic(typeof(XiaoTuCharacter))]
public sealed class RustyMemorialDice : XiaoTuRelic
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    //NOTE: unverified against sts2.dll in the cloud environment; if the hook name differs, fix it on the PC build.
    public override Task BeforeCombatStart()
    {
        var dice = DiceState.Of(Owner);
        dice.Clear();
        dice.Add(temporary: false);
        return Task.CompletedTask;
    }

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner) return Task.CompletedTask;

        var dice = DiceState.Of(player);
        dice.RemoveTemporary();
        dice.RerollAll();
        return Task.CompletedTask;
    }
}
