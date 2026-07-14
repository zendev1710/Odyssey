using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using Odyssey.Models.Localization;
using Odyssey.Services;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using static Odyssey.Models.Documents.CRDocument;
using static Odyssey.Models.Documents.SimpleItemSelection;
using static Odyssey.Models.ShipModel;
using static Odyssey.Models.Tools.DataProperty;
using static Odyssey.Utils.Converters;

namespace Odyssey.ViewModels;

public partial class NoDetailsViewModel : ContainerViewModel
{
    public NoDetailsViewModel(IEventAggregator? eventAggregator = null, IActiveDocumentService? documentService = null) : base(Ids.NoDetails, eventAggregator, documentService)
    {
    }

    protected override void OnSelectionChanged(ISelectionChange selectionChange)
    {
        // This view does not handle any selection changes.
    }
}

