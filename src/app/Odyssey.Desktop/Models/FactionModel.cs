using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using Odyssey.Models.Localization;
using System.Collections.Generic;

namespace Odyssey.Models;

public class FactionModel
{
    public DataBlock Data { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public int Id { get; private set; }
    public bool IsActive { get; private set; } = false;
    public int Recruitment { get; private set; } = 0;

    public Dictionary<int, DataBlock> Alliances { get; private set; } = [];

    public FactionModel(DataBlock data)
    {
        Data = data;
        Recruitment = data.ValueInt(KeyType.RECRUITMENTCOST);
        string name = data.Value(KeyType.FACTIONNAME);
        Name = string.IsNullOrEmpty(name) ? Labels.Localize(Labels.DISGUISED) : $"{name} ({data.IdToString()})";
        IsActive = CRDocument.FactionIsActive(data);
        Id = data.GetId();
    }

    public void AddAlliance(DataBlock ally, int blockId)
    {
        Alliances[blockId] = ally;
    }

    public override string ToString()
    {
        return Name;
    }
}
