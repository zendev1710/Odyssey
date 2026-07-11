using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Media;

namespace Odyssey.Controls;

/// <summary>
/// A toggleable star control (favorite/bookmark) inheriting from ToggleButton.
/// </summary>
[TemplatePart("PART_StarPresenter", typeof(Path))]
public class StarControl : ToggleButton
{
    private Path? _starPresenter;

    /// <summary>
    /// Defines the <see cref="StarFill"/> property.
    /// </summary>
    public static readonly DirectProperty<StarControl, IBrush?> StarFillProperty =
        AvaloniaProperty.RegisterDirect<StarControl, IBrush?>(
            nameof(StarFill),
            o => o.StarFill);

    public IBrush? StarFill => GetStarFill();

    public StarControl()
    {
        // When a new instance of the control is created, we need to update the star visible
        //UpdateStars();
    }

    private IBrush? GetStarFill()
    {
        var key = IsChecked == true ? "StarControlSelectedBrush" : "StarControlUnselectedBrush";
        return this.FindResource(key) as IBrush;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_starPresenter is not null)
        {
            _starPresenter.PointerReleased -= StarPresenter_PointerReleased;
        }

        _starPresenter = e.NameScope.Find("PART_StarPresenter") as Path;

        if (_starPresenter != null)
        {
            _starPresenter.PointerReleased += StarPresenter_PointerReleased;
        }
    }

    private void StarPresenter_PointerReleased(object? sender, Avalonia.Input.PointerReleasedEventArgs e)
    {
        // ToggleButton already handles IsChecked, but we can force toggle for custom visuals
        //IsChecked = !IsChecked;
        // Command is handled by ToggleButton, but you can add custom logic here if needed
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsCheckedProperty)
        {
            RaisePropertyChanged(StarFillProperty, null, StarFill);
        }
    }

    static StarControl()
    {
        // .Button or .Custom depending on your intent
        AutomationProperties.ControlTypeOverrideProperty.OverrideDefaultValue<StarControl>(AutomationControlType.Button);
    }
}