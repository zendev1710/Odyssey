using System;
using System.Collections.Generic;
using System.Text;

namespace Odyssey.Core.Help.Dto
{
    public class HelpTopicDto
    {
        public string Title { get; set; } = "";
        public string Id { get; set; } = "";
        public List<HelpTopicDto>? Children { get; set; }
    }
}
