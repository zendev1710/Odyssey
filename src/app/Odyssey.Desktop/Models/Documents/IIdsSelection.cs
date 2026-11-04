
namespace Odyssey.Models.Documents
{
    /// <summary>
    /// Represents an object that can be selected and provides information about its selection state.
    /// </summary>
    public interface IIdsSelection
    {
        public string RegionId { get; }

        public string FactionId { get; }

        public  string Item { get; }

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
