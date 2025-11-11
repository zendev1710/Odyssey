using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using Odyssey.Models.Localization;

namespace Odyssey.Models;

public class FactionModel
{
    public DataBlock Data { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public int Id { get; private set; }
    public bool IsActive { get; private set; } = false;
    public int Recruitment { get; private set; } = 0;

    public FactionModel(DataBlock dataBlock)
    {
        Data = dataBlock;
        Recruitment = Data.ValueInt(KeyType.RECRUITMENTCOST);
        string name = Data.Value(KeyType.FACTIONNAME);
        Name = string.IsNullOrEmpty(name) ? Labels.Localize(Labels.DISGUISED) : $"{name} ({Data.IdToString()})";
        IsActive = CRDocument.FactionIsActive(Data);
        Id = Data.GetId();
    }
    public override string ToString()
    {
        return Name;
    }
}
