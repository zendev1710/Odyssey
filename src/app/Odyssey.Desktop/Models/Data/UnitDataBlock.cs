using Newtonsoft.Json.Linq;
using Odyssey.Models.Documents;
using Odyssey.Models.Tools;

namespace Odyssey.Models.Data;

public class UnitDataBlock : DataBlock, IBookmarkable
{
    public UnitDataBlock(string id): base(id)
    {
        Type = BlockType.UNIT;
    }
}    
