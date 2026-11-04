using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Odyssey.Models.Data
{
    public class OrdersAttachment : IAttachment
    {
        public string Header { get; set; }
        public List<string> Commands { get; private set; }
        private List<string> PrefixLines { get; set; }
        private List<string> PostfixLines { get; set; }
        public OrdersAttachment()
        {
            Header = string.Empty;
            Commands = [];
            PostfixLines = [];
            PrefixLines = [];
        }
        public void Add(List<DataKey> dataKeys)
        {
            Commands = [];
            PostfixLines = [];
            foreach (var item in dataKeys)
            {
                Commands.Add(item.GetValue());
            }
        }
        public void Add(string line)
        {
            Commands.AddRange(PostfixLines);
            PostfixLines.Clear();
            Commands.Add(line);
        }
        public void Clear()
        {
            Header = string.Empty;
            Commands = [];
            PostfixLines = [];
            PrefixLines = [];
        }
    }
}
