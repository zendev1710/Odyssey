using Odyssey.Models.Tools;

namespace Odyssey.ViewModels;

public partial class TreeNodeViewModel : NodeViewModel
{
    public TreeNodeViewModel()
    {
    }

    public TreeNodeViewModel(NodeViewModel parent, DataProperty? data, string header = "", int index = -1) : base(parent, data, index)
    {
        if (!string.IsNullOrEmpty(header))
        {
            Header = header;
        }

        if (data == null)
        {
            Data = new DataProperty(header);
        }
    }
}
