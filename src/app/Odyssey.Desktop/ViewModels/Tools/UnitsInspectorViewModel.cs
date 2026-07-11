using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Prism.Events;
using Odyssey.Models.Documents;
using Avalonia.Controls;

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

        public UnitsInspectorViewModel(IEventAggregator? eventAggregator = null) : base(eventAggregator)
        {
            // create child viewmodels; they will get the real Report later via SetMapFile
            StripViewModel = new UnitStripViewModel(eventAggregator);
            InspectorViewModel = new UnitInspectorViewModel(eventAggregator);
        }

        /// <summary>
        /// Propagate document to children when the active document changes.
        /// </summary>
        protected override bool SetMapFile(CRDocument cr)
        {
            bool changed = base.SetMapFile(cr);
            /*
            // forward to children (protected method is accessible because we're a derived class)
            if (StripViewModel != null)
            {
                StripViewModel.SetMapFile(cr);
            }
            if (InspectorViewModel != null)
            {
                InspectorViewModel.SetMapFile(cr);
            }
            */
            return changed;
        }

        /// <summary>
        /// Forward selection change events to children so they can react.
        /// </summary>
        protected override void OnSelectionChanged(ISelectionChange selectionChange)
        {
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
    }
}