using CommunityToolkit.Mvvm.ComponentModel;
using Odyssey.Events;
using Odyssey.Models.Documents;
using Dock.Model.Mvvm.Controls;
using Prism.Events;
using static Odyssey.Models.Documents.CRDocument;

namespace Odyssey.ViewModels.Documents;

/// <summary>
/// View model for a Plane.
/// </summary>
public partial class PlaneViewModel : Document
{
    [ObservableProperty]
    public string name;

    private readonly IEventAggregator? _eventAggregator;

    protected IEventAggregator? EventAggregator { get { return _eventAggregator; } }
    public PlaneViewModel(IEventAggregator? eventAggregator = null)
    {
        // Astral or world
        Name = "";
        _eventAggregator = eventAggregator;
    }
}
