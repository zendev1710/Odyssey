using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Odyssey.Search
{
    public class SearchResult
    {
        public string Region { get; set; }
        public string Element { get; set; }
        public string Faction { get; set; }

        public SearchResult()
        {
            Region = string.Empty;
            Element = string.Empty;
            Faction = string.Empty;
        }
    }
}
