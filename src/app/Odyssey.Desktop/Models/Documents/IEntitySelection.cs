namespace Odyssey.Models.Documents
{
    /// <summary>
    /// Represents an object that can be selected and provides information about its selection state.
    /// </summary>
    public interface IEntitySelection
    {
        public RegionModel? Region { get; }

        public LocatedFactionModel? LocatedFaction { get; }

        public EntityModel? Item { get; }

        public object? Data { get; }

        public bool HasUnit();

        public bool HasFaction();

        public bool HasShip();

        public bool HasBuilding();

        /// <summary>
        /// Returns true when the "buildings" container node of a region is selected (list of the region's buildings).
        /// </summary>
        public bool HasBuildings();

        /// <summary>
        /// Returns true when the "ships" container node of a region is selected (list of the region's ships).
        /// </summary>
        public bool HasShips();

        public bool IsRegionSelected();

        public bool HasRegion();

        public bool HasOnlyRegion();

        public bool IsEmpty();
    }
}
