using Newtonsoft.Json.Linq;
using Odyssey.Models.Documents;
using Odyssey.Models.Tools;

namespace Odyssey.Models.Data;

public class BuildingDataBlock : DataBlock, IBookmarkable
{
    public BuildingDataBlock(string id) : base(id)
    {
        Type = BlockType.BUILDING;
    }
}
