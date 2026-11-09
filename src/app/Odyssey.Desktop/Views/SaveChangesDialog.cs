using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Odyssey.Assets;
using Odyssey.ViewModels.Documents;
using System.Globalization;
using System.Threading.Tasks;

namespace Odyssey.Views;

internal sealed class SaveChangesDialog : Window
{
    private SaveChangesDialog(string title, string message, bool showSaveButton)
    {
        Title = title;
        Width = 440;
        SizeToContent = SizeToContent.Height;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;

        StackPanel content = new()
        {
            Margin = new Thickness(20),
            Spacing = 16
        };
        content.Children.Add(new TextBlock
        {
            Text = message,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap
        });

        StackPanel buttons = new()
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 8
        };
        if (showSaveButton)
        {
            buttons.Children.Add(CreateButton(Lang.Yes, SaveChangesChoice.Save));
            buttons.Children.Add(CreateButton(Lang.No, SaveChangesChoice.Discard));
        }
        buttons.Children.Add(CreateButton(showSaveButton ? Lang.Cancel : Lang.Close, SaveChangesChoice.Cancel));
        content.Children.Add(buttons);
        Content = content;
    }

    public static Task<SaveChangesChoice> ShowAsync(Window owner, string documentTitle)
    {
        string title = Lang.save_changes_title;
        string message = string.Format(CultureInfo.CurrentCulture, Lang.save_changes_message, documentTitle);
        return new SaveChangesDialog(title, message, showSaveButton: true).ShowDialog<SaveChangesChoice>(owner);
    }

    public static async Task ShowSaveFailureAsync(Window owner, string documentTitle)
    {
        string title = Lang.save_changes_title;
        string message = string.Format(CultureInfo.CurrentCulture, Lang.save_changes_failed, documentTitle);
        await new SaveChangesDialog(title, message, showSaveButton: false).ShowDialog<SaveChangesChoice>(owner);
    }

    private Button CreateButton(string label, SaveChangesChoice result)
    {
        Button button = new() { Content = label };
        button.Click += (_, _) => Close(result);
        return button;
    }
}