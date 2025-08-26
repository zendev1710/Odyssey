using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using Dock.Model.Mvvm.Controls;

namespace Odyssey.ViewModels.Documents;

/// <summary>
/// View model for a file.
/// </summary>
public class FileViewModel : Document
{
    private string _path = string.Empty;
    private string _text = string.Empty;
    private string _encoding = string.Empty;
    private EresseaFileType _eresseaFileType = EresseaFileType.UNKNOWN;
    private EresseaDocument? _document;

    public FileViewModel()
    {
    }

    /// <summary>
    /// Overrides IDockable::OnClose.
    /// Close the active document.
    /// </summary>
    /// <returns>true to accept to close; optherwise, false.</returns>
    public override bool OnClose()
    {
        // TODO:
        // - a report document should be considered as modified if some orders have been edited and confirmed; in that case must be saved somewhere (orders file attached to the cr ?)
        // 
        // - if document is modified, ask (yes/no/cancel) if file has to be saved :
        //   - returns false on cancel
        //   - save if yes
        //   - do not save if no
        // - if it's the unique opened document, send a OnActiveDocumentChanged(null)
        return true;
    }

    /// <summary>
    /// The Eressea document.
    /// Can be a report document (for .cr file) or an orders document (.txt file).
    /// </summary>
    public EresseaDocument? Document
    {
        get => _document;
        set => SetProperty(ref _document, value);
    }

    /// <summary>
    /// Pathname of the file.
    /// </summary>
    public string Path
    {
        get => _path;
        set => SetProperty(ref _path, value);
    }

    /// <summary>
    /// Content of the file. If the file content is very big, this can stay empty.
    /// </summary>
    public string Text
    {
        get => _text;
        set => SetProperty(ref _text, value);
    }

    /// <summary>
    /// File encoding.
    /// </summary>
    public string Encoding
    {
        get => _encoding;
        set => SetProperty(ref _encoding, value);
    }

    /// <summary>
    /// TYpe of Eressea file.
    /// Can be a report file type, an orders file type, or an unknown file type.
    /// </summary>
    public EresseaFileType EresseaFileType
    {
        get => _eresseaFileType;
        set => SetProperty(ref _eresseaFileType, value);
    }
}

