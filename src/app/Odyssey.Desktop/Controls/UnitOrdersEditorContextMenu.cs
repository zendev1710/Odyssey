using Avalonia.Controls;
using AvaloniaEdit;

namespace Odyssey.Controls;

public static class UnitOrdersEditorContextMenu
{
    public static void Attach(TextEditor editor, bool allowPaste)
    {
        var menu = new ContextMenu();
        MenuItem? cutItem = null;
        if (allowPaste)
        {
            cutItem = new MenuItem
            {
                Header = "Cut",
                Command = AvaloniaEdit.ApplicationCommands.Cut
            };
            menu.Items.Add(cutItem);
        }

        var copyItem = new MenuItem
        {
            Header = "Copy",
            Command = AvaloniaEdit.ApplicationCommands.Copy
        };
        menu.Items.Add(copyItem);

        MenuItem? pasteItem = null;
        if (allowPaste)
        {
            pasteItem = new MenuItem
            {
                Header = "Paste",
                Command = AvaloniaEdit.ApplicationCommands.Paste
            };
            menu.Items.Add(pasteItem);
        }

        menu.Items.Add(new Separator());
        var selectAllItem = new MenuItem
        {
            Header = "Select All"
        };
        selectAllItem.Click += (_, _) => editor.SelectAll();
        menu.Items.Add(selectAllItem);

        menu.Opening += (_, _) =>
        {
            if (cutItem is not null)
            {
                cutItem.IsEnabled = !editor.IsReadOnly && editor.CanCopy;
            }
            copyItem.IsEnabled = editor.CanCopy;
            if (pasteItem is not null)
            {
                pasteItem.IsEnabled = !editor.IsReadOnly && editor.CanPaste;
            }
            selectAllItem.IsEnabled = editor.Document.TextLength > 0
                && editor.TextArea.Selection.Length < editor.Document.TextLength;
        };
        editor.ContextMenu = menu;
    }
}
