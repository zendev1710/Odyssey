using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using Odyssey.Utils;
using Prism.Events;
using System;
using System.Collections.Generic;

namespace Odyssey.ViewModels.Tools;

public partial class MapViewModel : DocumentToolViewModelBase
{
    [ObservableProperty]
    DataBlock? _selectedRegion;

    [ObservableProperty]
    Seasons _season = Seasons.UNKNOWN;

    partial void OnSelectedRegionChanged(DataBlock? oldValue, DataBlock? newValue)
    {
        if (oldValue != newValue && newValue != null)
        {
            ISelection selection = new SimpleItemSelection(item: newValue, region: newValue, faction: null);
            SetSelection(selection);
            PublishSelectionChangedEvent(new SelectionChange(selection, this, null));
        }
    }

    public IEnumerable<KeyValuePair<int, DataBlock>> Regions
    {
        get => _regions;
        set
        {
            if (!EqualityComparer<IEnumerable<KeyValuePair<int, DataBlock>>>.Default.Equals(_regions, value))
            {
                _regions = value;
                OnPropertyChanged(nameof(Regions));
            }
        }
    }
    private IEnumerable<KeyValuePair<int, DataBlock>> _regions = [];

    public MapViewModel() : this(null)
    {
        if (!Design.IsDesignMode)
        {
            throw new InvalidOperationException("This constructor should only be used in design mode.");
        }
    }

    public MapViewModel(IEventAggregator? eventAggregator) : base(eventAggregator)
    {
    }

    protected override void OnSelectionChanged(ISelectionChange selectionChange)
    {
        List<string> selectorIdsExcludeFilter = [Id];
        List<string> selectorIdsIncludeFilter = [Ids.Explorer];
        if (ShouldIgnoreSelectionChangedEvent(selectionChange, selectorIdsIncludeFilter, selectorIdsExcludeFilter))
        {
            return;
        }

        ISelection sel = selectionChange.Selection;
        if (sel.IsRegionSelected() && !IsSelectedRegion(sel))
        {
            SetSelection(sel);
            SelectedRegion = Selection?.Region!;
        }
    }

    /// <summary>
    /// Handle active document changed event.
    /// Rebuilds the tree from the new active document data.
    /// </summary>
    /// <param name="cr"></param>
    protected override void OnActiveDocumentChanged(CRDocument cr)
    {
        if (SetMapFile(cr))
        {
            RebuildMap();
        }
    }

    protected override void OnActiveDocumentClosed(CRDocument cr)
    {
        //Clear();
        // reset to an empty report document
        SetMapFile(new CRDocument());
        RebuildMap();
    }

    /// <summary>
    /// Rebuilds the map from the data in the report document.
    /// </summary>
    private void RebuildMap()
    {
        Regions = GetDocument().AllRegions;
        Season = DateUtils.GetGameSeason(GetDocument().Turn);
    }
}
