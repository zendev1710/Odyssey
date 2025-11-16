using CommunityToolkit.Mvvm.ComponentModel;
using Odyssey.Models.Data;
using Prism.Events;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using static Odyssey.Utils.Converters;
using static Odyssey.Models.Documents.CRDocument;
using static Odyssey.Models.Documents.SimpleItemSelection;
using static Odyssey.Models.ShipModel;
using Odyssey.Models.Localization;
using static Odyssey.Models.Tools.DataProperty;
using Avalonia.Controls;
using System;
using System.Diagnostics;
using Odyssey.Models.Documents;

namespace Odyssey.ViewModels;

public partial class NoDetailsViewModel : ContainerViewModel
{
    public NoDetailsViewModel(): this(null)
    {
        if (!Design.IsDesignMode)
        {
            throw new InvalidOperationException("This constructor should only be used in design mode.");
        }
    }

    public NoDetailsViewModel(IEventAggregator? eventAggregator) : base(Ids.NoDetails, eventAggregator)
    {
    }

    protected override void OnSelectionChanged(ISelectionChange selectionChange)
    {
        // This view does not handle any selection changes.
    }
}

