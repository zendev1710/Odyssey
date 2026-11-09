using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using Dock.Model.Mvvm.Controls;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Odyssey.ViewModels.Documents;

public enum SaveChangesChoice
{
    Cancel,
    Save,
    Discard
}

/// <summary>
/// View model for a file.
/// </summary>
public class FileViewModel : Document
{
    private string _path = string.Empty;
    private string _text = string.Empty;
    private string _encoding = string.Empty;
    private DocumentType _documentType = DocumentType.UNKNOWN;
    private EresseaDocument? _document;
    private bool _allowCloseOnce;
    private bool _closePromptPending;

    public Func<FileViewModel, Task<bool>>? ConfirmCloseAsync { get; set; }
    public Action<FileViewModel>? CloseApproved { get; set; }

    public FileViewModel()
    {
    }

    public bool IsDocumentModified()
    {
        return IsModified
            || (Document?.IsModified ?? false)
            || (Document is CRDocument report && report.HasEditedOrderData);
    }

    /// <summary>
    /// Overrides IDockable::OnClose.
    /// Close the active document.
    /// </summary>
    /// <returns>true to accept to close; optherwise, false.</returns>
    public override bool OnClose()
    {
        if (_allowCloseOnce)
        {
            _allowCloseOnce = false;
            return true;
        }

        if (!IsDocumentModified())
        {
            return true;
        }

        if (ConfirmCloseAsync is null || CloseApproved is null || _closePromptPending)
        {
            return false;
        }

        _closePromptPending = true;
        _ = CompleteCloseRequestAsync();
        return false;
    }

    public void AllowCloseOnce() => _allowCloseOnce = true;

    public void DiscardChanges()
    {
        if (Document is CRDocument report)
        {
            report.DiscardEditedOrderChanges();
        }

        Document?.SetModified(false);
        IsModified = false;
    }

    private async Task CompleteCloseRequestAsync()
    {
        try
        {
            if (await ConfirmCloseAsync!(this))
            {
                AllowCloseOnce();
                CloseApproved!(this);
            }
        }
        catch (Exception exception)
        {
            Debug.WriteLine($"[FILEVIEWMODEL] Unable to close '{Title}': {exception}");
        }
        finally
        {
            _closePromptPending = false;
        }
    }

    /// <summary>
    /// The Eressea document.
    /// Can be a report document (for .cr file) or an orders document (.txt file).
    /// </summary>
    public EresseaDocument? Document
    {
        get => _document;
        set
        {
            if (ReferenceEquals(_document, value))
            {
                return;
            }

            if (_document is CRDocument previousReport)
            {
                previousReport.ModifiedChanged -= OnReportModifiedChanged;
                previousReport.EditedOrderDataChanged -= OnEditedOrderDataChanged;
            }

            if (!SetProperty(ref _document, value))
            {
                return;
            }

            if (_document is CRDocument report)
            {
                report.ModifiedChanged += OnReportModifiedChanged;
                report.EditedOrderDataChanged += OnEditedOrderDataChanged;
                UpdateModifiedState(report);
            }
        }
    }

    private void OnReportModifiedChanged(object? sender, EventArgs e)
    {
        if (sender is CRDocument report)
        {
            UpdateModifiedState(report);
        }
    }

    private void OnEditedOrderDataChanged(object? sender, EventArgs e)
    {
        if (sender is CRDocument report)
        {
            UpdateModifiedState(report);
        }
    }

    private void UpdateModifiedState(CRDocument report)
    {
        IsModified = report.IsModified || report.HasEditedOrderData;
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
    public DocumentType DocumentType
    {
        get => _documentType;
        set => SetProperty(ref _documentType, value);
    }
}

