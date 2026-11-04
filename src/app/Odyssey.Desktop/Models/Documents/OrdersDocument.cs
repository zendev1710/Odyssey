using Odyssey.Models.Dal;
using Odyssey.Models.Data;
using System.Collections.Generic;
using TextMateSharp.Themes;

namespace Odyssey.Models.Documents
{
    public class OrdersDocument : EresseaDocument
    {
        public string Password { get; private set; }

        /// <summary>
        /// Command lines before any orders.
        /// </summary>
        public List<string> PrefixLines { get; private set; }

        /// <summary>
        /// Command lines in REGIONS
        /// </summary>
        public Dictionary<Coordinates, OrdersAttachment> RegionLines { get; private set; }

        public OrdersDocument(OrdersParser? parser = null)
        {
            Password = "";
            PrefixLines = [];
            RegionLines = [];

            if (parser != null)
            {
                Password = parser.Password;
                LinesNumber = parser.LinesNumber;
                Locale = parser.Locale;
                Text = parser.Text;
            }
        }
    }
}
