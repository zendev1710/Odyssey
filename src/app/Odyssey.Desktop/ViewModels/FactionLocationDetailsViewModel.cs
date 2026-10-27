using CommunityToolkit.Mvvm.ComponentModel;
using Odyssey.Models;
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using Odyssey.ViewModels.Tools;
using System.Diagnostics;

namespace Odyssey.ViewModels;

public partial class FactionLocationDetailsViewModel : ViewModelBase
{
    [ObservableProperty]
    private LocatedFactionModel? locatedFaction;

    [ObservableProperty]
    private string factionName = string.Empty;

    [ObservableProperty]
    private string regionName = string.Empty;

    [ObservableProperty]
    private string race = string.Empty;

    [ObservableProperty]
    private FactionStatus status = FactionStatus.UNKNOWN;

    public FactionLocationDetailsViewModel() : base(Ids.FactionLocationDetails)
    {
    }

    protected override void OnSelectionChanged(IEntitySelection selection, SelectionChangeTypes selType, object? sender)
    {
        if (ReferenceEquals(sender, this))
        {
            return;
        }

        if (selection.Data is not ExplorerNodeViewModel node ||
            node.NodeType != ExplorerNodeViewModel.NodeTypes.FACTION ||
            selection.LocatedFaction is null)
        {
            return;
        }

        LocatedFaction = selection.LocatedFaction;
        FactionName = LocatedFaction.Name;
        RegionName = LocatedFaction.Region.Name;
        Race = LocatedFaction.Race.ToString();
        Status = node.AdditionalInfo is FactionStatus factionStatus
            ? factionStatus
            : LocatedFaction.Faction?.Status ?? FactionStatus.UNKNOWN;

        Debug.WriteLine($"[VM-FACTION] Selected faction {FactionName} in {RegionName}");
    }
}
