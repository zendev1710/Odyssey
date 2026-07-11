using AvaloniaEdit.Editing;
using Odyssey.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Odyssey.Models.Documents.CRDocument;

namespace Odyssey.Models.Documents
{
    /// <summary>
    /// Represents an object that can be selected and provides information about its selection state and associated
    /// selectors.
    /// </summary>
    /// <remarks>This interface defines properties to access the current selection state and the selectors
    /// responsible for managing the entity's selection.</remarks>
    public interface ISelection
    {
        DataBlock? Region { get; }

        DataBlock? Faction { get; }

        DataBlock? Item { get; }

        bool IsUnitSelected();

        bool IsShipSelected();

        bool IsBuildingSelected();

        bool IsRegionSelected();

        bool IsFactionSelected();

        bool HasFaction();

        bool HasRegion();

        bool NothingSelected();
    }
}
