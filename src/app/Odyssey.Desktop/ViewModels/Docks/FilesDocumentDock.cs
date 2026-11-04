using System.Text;
using CommunityToolkit.Mvvm.Input;
using Dock.Model.Mvvm.Controls;
using Odyssey.Models.Data;
using Odyssey.ViewModels.Documents;

namespace Odyssey.ViewModels.Docks;

public class FilesDocumentDock : DocumentDock
{
    public FilesDocumentDock()
    {
        CreateDocument = new RelayCommand(CreateNewDocument);
    }

    private void CreateNewDocument()
    {
        if (!CanCreateDocument)
        {
            return;
        }

        var index = VisibleDockables?.Count + 1;
        var document = new FileViewModel()
        {
            Id = $"Document{index}",
            Title = $"Untitled{index}",
            Path = string.Empty,
            Text = "",
            Encoding = Encoding.Default.WebName,
            DocumentType = DocumentType.UNKNOWN
        };

        Factory?.AddDockable(this, document);
        Factory?.SetActiveDockable(document);
        Factory?.SetFocusedDockable(this, document);
    }
}
