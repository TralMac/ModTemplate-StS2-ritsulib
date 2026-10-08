using RitsuChar.RitsuCharCode.Extensions;
using STS2RitsuLib.Scaffolding.Content;

namespace RitsuChar.RitsuCharCode.Character;

public sealed class RitsuCharPotionPool : TypeListPotionPoolModel
{
    public override string EnergyColorName => "ritsuchar";

    public override string? TextEnergyIconPath => "charui/text_energy.png".ImagePath();
    public override string? BigEnergyIconPath => "charui/big_energy.png".ImagePath();
}
