using Odyssey.Models.Documents;
using System.Diagnostics;

namespace Odyssey.ViewModels.Tools
{
    /// <summary>
    /// Composite ViewModel for the UnitsInspector view.
    /// Hosts a vertical UnitStripViewModel and a UnitInspectorViewModel and forwards the active document/selection.
    /// </summary>
    public partial class UnitsInspectorViewModel : DocumentToolViewModelBase
    {
        public UnitStripViewModel StripViewModel { get; }
        public UnitInspectorViewModel InspectorViewModel { get; }

        // Parameterless ctor for design time

        public UnitsInspectorViewModel() : base()
        {
            // create child view models; they will get the real Report later via SetMapFile
            StripViewModel = new UnitStripViewModel();
            InspectorViewModel = new UnitInspectorViewModel();
        }

        /// <summary>
        /// Forward selection change events to children so they can react.
        /// </summary>
        protected override void OnSelectionChanged(IEntitySelection selection, SelectionChangeTypes selType, object? sender)
        {
            // If event come from this viewModel, ignore it
            if (ReferenceEquals(sender, this)) return;

            bool regionChanged = selType.HasFlag(SelectionChangeTypes.Region);
            bool regionSelected = selection.IsRegionSelected();

            Debug.WriteLine($"[VM-UNITS-INSPECTOR] OnSelectionChanged: region changed={regionChanged}, region selected={regionSelected}");

            if (regionChanged)
            {
                HandleSelection(selection);
            }

            // TODO

            /*
            // prevent loops: respect ignore rules if any
            // forward to children so they update their presented units/inspector
            try
            {
                StripViewModel?.OnSelectionChanged(selectionChange);
            }
            catch { }
            try
            {
                InspectorViewModel?.OnSelectionChanged(selectionChange);
            }
            catch {  }
            */
        }

        private void HandleSelection(IEntitySelection sel)
        {
            // TODO: update units list when region selection changes
        }
    }
}