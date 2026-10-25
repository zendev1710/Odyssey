namespace Odyssey.Models.Tools
{
    public class BattleModel
    {
        private readonly string _content;
        private readonly string _label;
        private readonly string _factionName;
        private readonly int _x;
        private readonly int _y;
        private readonly int _plane;

        public string Content { get { return _content; } }

        public string Label { get { return _label; } }

        public string FactionName { get { return _factionName; } }

        public int X { get { return _x; } }
        public int Y { get { return _y; } }
        public int Plane { get { return _plane; } }

        public BattleModel(string label, string factionName, int x, int y, int plane, in string content)
        {
            _factionName = factionName;
            _x = x;
            _y = y;
            _plane = plane;
            _content = content;
            _label = label;
        }

        public override string ToString()
        {
            return Label!;
        }
    }
}
