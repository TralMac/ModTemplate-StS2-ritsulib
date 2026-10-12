using XiaoTu.XiaoTuCode.Extensions;
using STS2RitsuLib.Scaffolding.Content;

namespace XiaoTu.XiaoTuCode.Character;

public sealed class XiaoTuRelicPool : TypeListRelicPoolModel
{
    public override string EnergyColorName => "xiaotu";

    public override string? TextEnergyIconPath => "charui/text_energy.png".ImagePath();
    public override string? BigEnergyIconPath => "charui/big_energy.png".ImagePath();
}
