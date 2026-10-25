using Newtonsoft.Json.Linq;
using Odyssey.Models.Documents;
using Odyssey.Models.Tools;

namespace Odyssey.Models.Data;

public class IslandDataBlock : DataBlock, IBookmarkable
{
    public IslandDataBlock(string id): base(id)
    {
        Type = BlockType.ISLAND;
    }
}    
