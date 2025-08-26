namespace Odyssey.Help
{
    public class HelpTopic
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Icon { get; set; }
        public int Order { get; set; }
        public string Language { get; set; }

        public HelpTopic(string id, string title, string icon, int order, string language)
        {
            Id = id;
            Title = title;
            Icon = icon;
            Order = order;
            Language = language;
        }
    }
}