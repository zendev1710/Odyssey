using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Odyssey.Models.Documents;
using Odyssey.Services;
using Prism.Events;
using System;

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

        public UnitsInspectorViewModel(IEventAggregator? eventAggregator = null, IActiveDocumentService? documentService = null) : base(eventAggregator, documentService)
        {
            // create child viewmodels; they will get the real Report later via SetMapFile
            StripViewModel = new UnitStripViewModel(eventAggregator);
            InspectorViewModel = new UnitInspectorViewModel(eventAggregator);
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