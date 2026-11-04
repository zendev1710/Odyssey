using System.Collections.ObjectModel;

namespace Odyssey.Core.Help.Models
{
    public class HelpTocItem
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public ObservableCollection<HelpTocItem> Children { get; set; } = [];

        public HelpTocItem() { }

        public HelpTocItem(string id, string title)
        {
            Id = id;
            Title = title;
        }

        // Optionnel : computed property
        public string FileName => $"{Id}.md";
    }
}