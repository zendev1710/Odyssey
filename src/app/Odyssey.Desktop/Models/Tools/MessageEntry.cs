using Odyssey.Models.Documents;
using System;
using System.Collections.Generic;
using Odyssey.Models.Data;

namespace Odyssey.Models.Tools
{
    public class MessageEntry
    {
        public string? Label { get; private set; }

        public MessageTargetsSelection? Selection { get; }

        public MessageEntry(String label, List<DataBlock> targets)
        {

            Label = label;
            Selection = new MessageTargetsSelection(targets);
        }

        public override string ToString()
        {
            return Label!;
        }
    }
}
