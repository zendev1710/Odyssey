using Odyssey.Models.Documents;
using System;

namespace Odyssey.Core.Services;

public interface IDocumentService
{
    CRDocument ActiveDocument { get; }
    event Action<CRDocument> ActiveDocumentChanged;
    event Action<CRDocument> ActiveDocumentReady;

    void SetActiveDocument(CRDocument document);
    void ClearDocument();
}

public class DocumentService : IDocumentService
{
    private CRDocument _activeDocument = new();

    public CRDocument ActiveDocument => _activeDocument;

    public event Action<CRDocument> ActiveDocumentChanged;
    public event Action<CRDocument> ActiveDocumentReady;

    public void SetActiveDocument(CRDocument document)
    {
        if (_activeDocument == document || _activeDocument.Name == document?.Name)
            return;

        _activeDocument = document ?? new CRDocument();
        ActiveDocumentChanged?.Invoke(_activeDocument);
        ActiveDocumentReady?.Invoke(_activeDocument);
    }

    public void ClearDocument()
    {
        var emptyDoc = new CRDocument();
        if (_activeDocument != emptyDoc && _activeDocument.Name != emptyDoc.Name)
        {
            _activeDocument = emptyDoc;
            ActiveDocumentChanged?.Invoke(_activeDocument);
            ActiveDocumentReady?.Invoke(_activeDocument);
        }
    }
}
