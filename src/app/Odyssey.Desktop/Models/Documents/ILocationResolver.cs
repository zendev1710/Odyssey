using Odyssey.Models.Data;

namespace Odyssey.Models.Documents;

public interface ILocationResolver
{
    public bool ResolveLocation(int x, int y, PlaneType plane, out ILocation? location);

    /// <summary>
    /// Resolves the entity model (region, unit, ship or building) identified by the specified id.
    /// </summary>
    /// <param name="id">The id of the entity. For regions it is the region id (also used as location id);
    /// for units, ships and buildings it is the block id.</param>
    /// <param name="item">When this method returns, contains the resolved entity model if found; otherwise <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if an entity model matching the id is found; otherwise <see langword="false"/>.</returns>
    public bool ResolveItem(int id, out EntityModel? item);
}