using Odyssey.Models.Documents;

namespace Odyssey.Models.Tools
{
    public class SelectionEntry
    {
        public string? Label { get; private set; }

        public ISelection? Selection { get; }

        public SelectionEntry(ISelection sel)
        {
            Label = sel.Item?.GetUILabel() ?? string.Empty;
            Selection = sel;
        }

        public override string ToString()
        {
            return Label!;
        }
    }
}
