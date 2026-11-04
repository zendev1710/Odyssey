using Odyssey.Models.Data;

namespace Odyssey.Models.Documents
{
    /// <summary>
    /// Represents an object that can be selected and provides information about its selection state.
    /// </summary>
    public interface ISelection
    {
        public RegionDataBlock? Region { get; }

        public DataBlock? Faction { get; }

        public DataBlock? Item { get; }

        public bool HasUnit();

        public bool HasShip();

        public bool HasBuilding();

        public bool IsRegionSelected();

        public bool IsFactionSelected();

        public bool HasFaction();

        public bool HasRegion();

        public bool IsEmpty();
    }
}
