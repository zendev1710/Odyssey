using Odyssey.Events;
using Odyssey.Models.Documents;
using Dock.Model.Mvvm.Controls;
using Prism.Events;
using static Odyssey.Models.Documents.CRDocument;

namespace Odyssey.ViewModels.Documents;

public class MapViewModel : Document
{
    private readonly IEventAggregator? _eventAggregator;
    private CRDocument Cr { get; set; }
    protected IEventAggregator? EventAggregator { get { return _eventAggregator; } }
    public MapViewModel(IEventAggregator? eventAggregator = null)
    {
        _eventAggregator = eventAggregator;
    }
}
