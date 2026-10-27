using Odyssey.Models.Documents;

namespace Odyssey.Models.Tools
{
    public class SelectionEntry
    {
        public string? Label { get; private set; }

        public IEntitySelection? Selection { get; }

        public SelectionEntry(IEntitySelection sel)
        {
            Label = sel.HasFaction() && !sel.HasUnit() && sel.LocatedFaction is not null
                ? $"{sel.LocatedFaction.Name} @ {sel.LocatedFaction.Region.Name}"
                : sel.Item?.DataBlock.GetUILabel() ?? string.Empty;
            Selection = CopyOf(sel);
        }

        /// <summary>
        /// Copies the specified selection.
        /// The selection instance owned by the selection service is reused for each selection (it is
        /// cleared then filled again): a copy has to be stored, otherwise the entry would always reflect
        /// the current selection instead of the one it has been created with.
        /// </summary>
        private static EntitySelection CopyOf(IEntitySelection sel)
        {
            EntitySelection copy = new();

            // A container selection (the buildings/ships list of a region) has no item.
            if (sel.HasBuildings() || sel.HasShips())
            {
                copy.SetContainer(EntitySelection.GetContainerTypes(sel), sel.Region, sel.Data);
            }
            else
            {
                copy.SetItem(sel.Item, sel.Region, sel.Data, locatedFaction: sel.LocatedFaction);
            }

            return copy;
        }

        public override string ToString()
        {
            return Label!;
        }
    }
}
