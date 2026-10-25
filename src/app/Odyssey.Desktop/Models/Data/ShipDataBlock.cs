using Newtonsoft.Json.Linq;
using Odyssey.Models.Documents;
using Odyssey.Models.Tools;

namespace Odyssey.Models.Data;

public class ShipDataBlock : DataBlock, IBookmarkable
{
    public ShipDataBlock(string id) : base(id)
    {
        Type = BlockType.SHIP;
    }
}
