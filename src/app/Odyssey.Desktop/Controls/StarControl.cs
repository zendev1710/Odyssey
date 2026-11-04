using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Shapes;

namespace Odyssey.Controls;

/// <summary>
/// A star button used to display and to change the bookmark state of the current selection.
/// The star is filled ("starred") when <see cref="IsStarred"/> is true (see StarStyles.axaml).
/// This control is a plain button: it has no internal checked state. The bookmark state belongs to the
/// view model, which sets <see cref="IsStarred"/>; clicking the control only executes its command.
/// </summary>
[PseudoClasses(StarredPseudoClass)]
[TemplatePart("PART_StarPresenter", typeof(Path))]
public class StarControl : Button
{
    private const string StarredPseudoClass = ":starred";

    /// <summary>
    /// Defines the <see cref="IsStarred"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsStarredProperty =
        AvaloniaProperty.Register<StarControl, bool>(nameof(IsStarred));

    /// <summary>
    /// Gets or sets whether the star is displayed as starred (filled), i.e. whether the current
    /// selection is a bookmark.
    /// </summary>
    public bool IsStarred
    {
        get => GetValue(IsStarredProperty);
        set => SetValue(IsStarredProperty, value);
    }

    static StarControl()
    {
        AutomationProperties.ControlTypeOverrideProperty.OverrideDefaultValue<StarControl>(AutomationControlType.Button);
        IsStarredProperty.Changed.AddClassHandler<StarControl>((control, args) => control.UpdatePseudoClasses(args.GetNewValue<bool>()));
    }

    public StarControl()
    {
        UpdatePseudoClasses(IsStarred);
    }

    private void UpdatePseudoClasses(bool isStarred)
    {
        PseudoClasses.Set(StarredPseudoClass, isStarred);
    }
}
