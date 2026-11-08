using System;
using System.Globalization;
using System.IO;
using AsyncImageLoader;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Odyssey.ViewModels;
using Odyssey.ViewModels.Tools;
using Odyssey.Settings;
using System.ComponentModel;
using System.Windows.Input;
using Odyssey.Converters;

namespace Odyssey.Controls;

public sealed class UnitItemBadgeControl : UserControl
{
    private const double BadgeSize = 60;
    private readonly Viewbox _viewbox;
    private readonly Border _badge;
    private readonly GlobalSettings _settings = GlobalSettings.Shared;
    private double _zoom = 1;

    public double Zoom
    {
        get => _zoom;
        set
        {
            if (!double.IsFinite(value) || value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Zoom must be a positive finite value.");
            }

            _zoom = Math.Min(value, 1);
            _viewbox.Width = BadgeSize * _zoom;
            _viewbox.Height = BadgeSize * _zoom;
        }
    }

    public UnitItemBadgeControl()
    {
        var image = new AdvancedImage((Uri)null!)
        {
            Width = 24,
            Height = 24,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Stretch = Stretch.Uniform,
        };
        image.Bind(AdvancedImage.SourceProperty, new Binding(nameof(UnitItemViewModel.IconPath))
        {
            Converter = IconAssetRasterUriConverter.Instance,
        });
        image.Bind(Visual.IsVisibleProperty, new Binding(nameof(UnitItemViewModel.IconPath))
        {
            Converter = IconPathFormatVisibilityConverter.Raster,
        });

        var themeColors = new Border { Width = 0, Height = 0 };
        themeColors.Bind(Border.BackgroundProperty, new DynamicResourceExtension("ApplicationThemeSurfaceBrush"));
        themeColors.Bind(Border.TagProperty, new DynamicResourceExtension("ApplicationThemeForegroundBrush"));

        var vectorImage = new Image
        {
            Width = 24,
            Height = 24,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Stretch = Stretch.Uniform,
        };
        var vectorSource = new MultiBinding { Converter = new IconPathToImageConverter() };
        vectorSource.Bindings.Add(new Binding(nameof(UnitItemViewModel.IconPath)));
        vectorSource.Bindings.Add(new Binding(nameof(Border.Background)) { Source = themeColors });
        vectorSource.Bindings.Add(new Binding(nameof(Border.Tag)) { Source = themeColors });
        vectorImage.Bind(Image.SourceProperty, vectorSource);
        vectorImage.Bind(Visual.IsVisibleProperty, new Binding(nameof(UnitItemViewModel.IconPath))
        {
            Converter = IconPathFormatVisibilityConverter.Svg,
        });

        var itemName = new TextBlock
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            FontWeight = FontWeight.SemiBold,
            Foreground = new SolidColorBrush(Color.Parse("#1a1a1a")),
        };
        itemName.Bind(TextBlock.TextProperty, new Binding(nameof(UnitItemViewModel.Name)));
        itemName.Bind(Visual.IsVisibleProperty, new Binding(nameof(UnitItemViewModel.IconPath))
        {
            Converter = IconVisibilityConverter.Hidden,
        });

        var quantity = new TextBlock
        {
            FontSize = 12,
            FontFamily = "Segoe UI",
            FontWeight = FontWeight.Black,
            Foreground = new SolidColorBrush(Color.Parse("#101010")),
            TextAlignment = TextAlignment.Center,
            MinWidth = 14,
        };
        quantity.Bind(TextBlock.TextProperty, new Binding(nameof(UnitItemViewModel.QuantityText)));

        var quantityBadge = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#f5d67b")),
            Opacity = 0.96,
            MinWidth = 26,
            MinHeight = 20,
            CornerRadius = new CornerRadius(10),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom,
            Margin = new Thickness(0, 0, 0, 5),
            Padding = new Thickness(5, 0),
            BorderBrush = new SolidColorBrush(Color.Parse("#7a6146")),
            BorderThickness = new Thickness(1),
            Child = quantity,
        };

        var grid = new Grid
        {
            RowDefinitions = new RowDefinitions("*,Auto"),
        };
        Grid.SetRow(image, 0);
        Grid.SetRow(itemName, 0);
        Grid.SetRow(quantityBadge, 1);
        grid.Children.Add(image);
        grid.Children.Add(vectorImage);
        grid.Children.Add(themeColors);
        grid.Children.Add(itemName);
        grid.Children.Add(quantityBadge);

        _badge = new Border
        {
            Width = BadgeSize,
            Height = BadgeSize,
            CornerRadius = new CornerRadius(10),
            BorderBrush = new SolidColorBrush(Color.Parse("#8f6d42")),
            BorderThickness = new Thickness(1.2),
            Background = new SolidColorBrush(Color.Parse("#A6A39E")),
            Child = grid,
        };
        _badge.Bind(ToolTip.TipProperty, new Binding(nameof(UnitItemViewModel.TooltipLabel)));
        UpdateBadgeBackground();
        AttachedToVisualTree += OnAttachedToVisualTree;
        DetachedFromVisualTree += OnDetachedFromVisualTree;

        _viewbox = new Viewbox
        {
            Width = BadgeSize,
            Height = BadgeSize,
            Stretch = Stretch.Uniform,
            Child = _badge,
        };
        Content = _viewbox;
    }

internal sealed class IconAssetRasterUriConverter : IValueConverter
{
    public static IconAssetRasterUriConverter Instance { get; } = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string iconPath || Path.GetExtension(iconPath).Equals(".svg", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return IconAssetUriConverter.Instance.Convert(value, targetType, parameter, culture);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

internal sealed class IconPathFormatVisibilityConverter : IValueConverter
{
    private readonly bool _isSvg;

    private IconPathFormatVisibilityConverter(bool isSvg) => _isSvg = isSvg;

    public static IconPathFormatVisibilityConverter Svg { get; } = new(true);
    public static IconPathFormatVisibilityConverter Raster { get; } = new(false);

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is string iconPath && !string.IsNullOrWhiteSpace(iconPath) &&
        Path.GetExtension(iconPath).Equals(".svg", StringComparison.OrdinalIgnoreCase) == _isSvg;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        _settings.SettingChanged += OnSettingChanged;
        UpdateBadgeBackground();
        if (DataContext is UnitItemViewModel item)
        {
            item.RefreshIconPath();
        }
    }

    private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e) =>
        _settings.SettingChanged -= OnSettingChanged;

    private void OnSettingChanged(object? sender, SettingChangedEventArgs e)
    {
        if (e.SettingName == GlobalSettings.UNIT_ITEM_BADGE_TRANSPARENT)
        {
            UpdateBadgeBackground();
        }

        if (e.SettingName == GlobalSettings.UNIT_ITEM_ICON_ASSET_FORMAT && DataContext is UnitItemViewModel item)
        {
            item.RefreshIconPath();
        }
    }

    private void UpdateBadgeBackground()
    {
        bool transparent = _settings.UnitItemBadgeTransparent;
        _badge.Background = transparent
            ? Brushes.Transparent
            : new SolidColorBrush(Color.Parse("#A6A39E"));
        _badge.BorderThickness = transparent ? new Thickness(0) : new Thickness(1.2);
    }
}

public sealed class StructureIdentityCardControl : UserControl
{
    public StructureIdentityCardControl()
    {
        var name = new SelectableTextBlock
        {
            FontWeight = FontWeight.SemiBold,
            TextWrapping = TextWrapping.NoWrap,
            TextTrimming = TextTrimming.CharacterEllipsis,
        };
        name.Bind(TextBlock.TextProperty, new Binding(nameof(StructureViewModel.Name)));

        var id = new SelectableTextBlock
        {
            FontSize = 10,
            TextWrapping = TextWrapping.NoWrap,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Margin = new Thickness(0, 3, 0, 0),
        };
        id.Bind(TextBlock.TextProperty, new Binding(nameof(StructureViewModel.StructureId)));

        var ownerLabel = new SelectableTextBlock { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
        ownerLabel.Bind(TextBlock.TextProperty, new Binding(nameof(StructureViewModel.OwnerLabel)));

        var colon = new SelectableTextBlock { Text = ":", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };

        var ownerLinkText = new SelectableLinkTextBlock
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            TextWrapping = TextWrapping.NoWrap,
            TextTrimming = TextTrimming.CharacterEllipsis,
        };
        ownerLinkText.Bind(TextBlock.TextProperty, new Binding(nameof(StructureViewModel.OwnerName)));
        ownerLinkText.Bind(SelectableLinkTextBlock.CommandProperty, new Binding(nameof(StructureViewModel.SelectOwnerCommand)));
        ownerLinkText.Bind(ToolTip.TipProperty, new Binding(nameof(StructureViewModel.OwnerTooltip)));
        ownerLinkText.Bind(Visual.IsVisibleProperty, new Binding(nameof(StructureViewModel.HasOwnerUnit)));

        var unlinkedOwner = new SelectableTextBlock { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
        unlinkedOwner.Bind(TextBlock.TextProperty, new Binding(nameof(StructureViewModel.OwnerName)));
        unlinkedOwner.Bind(Visual.IsVisibleProperty, new Binding(nameof(StructureViewModel.HasUnlinkedOwner)));

        var ownerRow = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,Auto,*"),
            Margin = new Thickness(0, 8, 0, 0),
            Children = { ownerLabel, colon, ownerLinkText, unlinkedOwner },
        };
        Grid.SetColumn(ownerLabel, 0);
        Grid.SetColumn(colon, 1);
        Grid.SetColumn(ownerLinkText, 2);
        Grid.SetColumn(unlinkedOwner, 2);
        ownerRow.Bind(Visual.IsVisibleProperty, new Binding(nameof(StructureViewModel.HasOwner)));

        var content = new StackPanel
        {
            Spacing = 3,
            Children = { name, id, ownerRow },
        };

        var card = new Border
        {
            Padding = new Thickness(6),
            Background = Brushes.Transparent,
            CornerRadius = new CornerRadius(12),
            Child = content,
        };
        Content = card;
    }
}

public sealed class StructureTypeSizeControl : UserControl
{
    public StructureTypeSizeControl()
    {
        var typeIcon = new AdvancedImage((Uri)null!)
        {
            Width = 28,
            Height = 28,
            Stretch = Stretch.Uniform,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
        };
        typeIcon.Bind(AdvancedImage.SourceProperty, new Binding(nameof(StructureViewModel.StructureTypeIconPath))
        {
            Converter = IconAssetUriConverter.Instance,
        });
        typeIcon.Bind(Visual.IsVisibleProperty, new Binding(nameof(StructureViewModel.StructureTypeIconPath))
        {
            Converter = IconVisibilityConverter.Visible,
        });
        typeIcon.Bind(ToolTip.TipProperty, new Binding(nameof(StructureViewModel.StructureTypeTooltip)));

        var size = new SelectableTextBlock
        {
            FontSize = 20,
            FontWeight = FontWeight.Black,
            TextAlignment = TextAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom,
        };
        size.Bind(TextBlock.TextProperty, new Binding(nameof(StructureViewModel.Size)));
        size.Bind(Visual.IsVisibleProperty, new Binding(nameof(StructureViewModel.HasSize)));
        size.Bind(ToolTip.TipProperty, new Binding(nameof(StructureViewModel.SizeLabel)));

        Content = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Spacing = 8,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
            Margin = new Thickness(8, 6, 6, 0),
            Children = { typeIcon, size },
        };
    }
}

public sealed class SelectableLinkTextBlock : SelectableTextBlock
{
    public static readonly StyledProperty<ICommand?> CommandProperty =
        AvaloniaProperty.Register<SelectableLinkTextBlock, ICommand?>(nameof(Command));

    public static readonly StyledProperty<object?> CommandParameterProperty =
        AvaloniaProperty.Register<SelectableLinkTextBlock, object?>(nameof(CommandParameter));

    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public SelectableLinkTextBlock()
    {
        TextDecorations = Avalonia.Media.TextDecorations.Underline;
        Cursor = new Cursor(StandardCursorType.Hand);
        Focusable = true;
        IsTabStop = true;
        Tapped += OnTapped;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key is Key.Enter or Key.Space)
        {
            ExecuteCommand();
            e.Handled = true;
            return;
        }

        base.OnKeyDown(e);
    }

    private void OnTapped(object? sender, TappedEventArgs e) => ExecuteCommand();

    private void ExecuteCommand()
    {
        if (Command?.CanExecute(CommandParameter) == true)
        {
            Command.Execute(CommandParameter);
        }
    }
}

public sealed class UnitSkillBadgeControl : UserControl
{
    private const double BadgeSize = 60;
    private readonly Viewbox _viewbox;
    private readonly Border _badge;
    private readonly Border _changeOverlay;
    private readonly Border _levelBadge;
    private readonly TextBlock _level;
    private readonly GlobalSettings _settings = GlobalSettings.Shared;
    private UnitSkillViewModel? _skillModel;
    private double _zoom = 1;

    public double Zoom
    {
        get => _zoom;
        set
        {
            if (!double.IsFinite(value) || value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Zoom must be a positive finite value.");
            }

            _zoom = Math.Min(value, 1);
            _viewbox.Width = BadgeSize * _zoom;
            _viewbox.Height = BadgeSize * _zoom;
        }
    }

    public UnitSkillBadgeControl()
    {
        _changeOverlay = new Border { CornerRadius = new CornerRadius(BadgeSize / 2) };

        var image = new AdvancedImage((Uri)null!)
        {
            Width = 22,
            Height = 22,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Stretch = Stretch.Uniform,
        };
        image.Bind(AdvancedImage.SourceProperty, new Binding(nameof(UnitSkillViewModel.IconPath))
        {
            Converter = IconAssetUriConverter.Instance,
        });
        image.Bind(Visual.IsVisibleProperty, new Binding(nameof(UnitSkillViewModel.IconPath))
        {
            Converter = IconVisibilityConverter.Visible,
        });

        var skillName = new TextBlock
        {
            Margin = new Thickness(4, 4, 4, 0),
            FontSize = 9,
            FontWeight = FontWeight.SemiBold,
            TextAlignment = TextAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Foreground = new SolidColorBrush(Color.Parse("#12313f")),
        };
        skillName.Bind(TextBlock.TextProperty, new Binding(nameof(UnitSkillViewModel.Name)));
        skillName.Bind(Visual.IsVisibleProperty, new Binding(nameof(UnitSkillViewModel.IconPath))
        {
            Converter = IconVisibilityConverter.Hidden,
        });

        _level = new TextBlock
        {
            FontSize = 14,
            FontWeight = FontWeight.Black,
            Foreground = Brushes.White,
            TextAlignment = TextAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
        };
        _levelBadge = new Border
        {
            MinWidth = 26,
            MinHeight = 22,
            CornerRadius = new CornerRadius(11),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom,
            Margin = new Thickness(0, 0, 0, 3),
            Padding = new Thickness(4, 0),
            Child = _level,
        };
        _levelBadge.Bind(Border.BackgroundProperty, new Binding(nameof(UnitSkillViewModel.LevelBackground)));

        var grid = new Grid { RowDefinitions = new RowDefinitions("*,Auto") };
        grid.Children.Add(_changeOverlay);
        Grid.SetRow(image, 0);
        Grid.SetRow(skillName, 0);
        Grid.SetRow(_levelBadge, 1);
        grid.Children.Add(image);
        grid.Children.Add(skillName);
        grid.Children.Add(_levelBadge);

        _badge = new Border
        {
            Width = BadgeSize,
            Height = BadgeSize,
            CornerRadius = new CornerRadius(BadgeSize / 2),
            BorderThickness = new Thickness(3),
            Background = new SolidColorBrush(Color.Parse("#B8B8B8")),
            Child = grid,
        };
        _badge.Bind(ToolTip.TipProperty, new Binding(nameof(UnitSkillViewModel.TooltipLabel)));
        UpdateBadgeAppearance();

        DataContextChanged += (_, _) =>
        {
            if (_skillModel is not null)
            {
                _skillModel.PropertyChanged -= OnSkillModelPropertyChanged;
            }

            _skillModel = DataContext as UnitSkillViewModel;
            if (_skillModel is not null)
            {
                _skillModel.PropertyChanged += OnSkillModelPropertyChanged;
            }

            UpdateBadgeAppearance();
            UpdateLevelText();
        };
        AttachedToVisualTree += OnAttachedToVisualTree;
        DetachedFromVisualTree += OnDetachedFromVisualTree;

        _viewbox = new Viewbox
        {
            Width = BadgeSize,
            Height = BadgeSize,
            Stretch = Stretch.Uniform,
            Child = _badge,
        };
        Content = _viewbox;
    }

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e) =>
        _settings.SettingChanged += OnSettingChanged;

    private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e) =>
        _settings.SettingChanged -= OnSettingChanged;

    private void OnSettingChanged(object? sender, SettingChangedEventArgs e)
    {
        if (e.SettingName is GlobalSettings.UNIT_SKILL_BADGE_SHOW_LEVEL_PREFIX or GlobalSettings.UNIT_SKILL_BADGE_TRANSPARENT)
        {
            UpdateBadgeAppearance();
            UpdateLevelText();
        }
    }

    private void OnSkillModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(UnitSkillViewModel.Change))
        {
            UpdateBadgeAppearance();
        }

        if (e.PropertyName is nameof(UnitSkillViewModel.Level) or nameof(UnitSkillViewModel.LevelText))
        {
            UpdateLevelText();
        }
    }

    private void UpdateBadgeAppearance()
    {
        bool transparent = _settings.UnitSkillBadgeTransparent;
        bool skillLevelChanged = _skillModel?.Change != 0;

        _badge.Background = transparent
            ? Brushes.Transparent
            : new SolidColorBrush(Color.Parse("#B8B8B8"));
        _badge.BorderBrush = _skillModel?.ChangeBorderBrush ?? Brushes.Transparent;
        _badge.BorderThickness = transparent && !skillLevelChanged
            ? new Thickness(0)
            : _skillModel?.ChangeBorderThickness ?? new Thickness(0);
        _changeOverlay.Background = transparent
            ? Brushes.Transparent
            : _skillModel?.ChangeCircleOverlay ?? Brushes.Transparent;
        _levelBadge.Background = transparent
            ? Brushes.Transparent
            : new SolidColorBrush(Color.Parse(_skillModel?.LevelBackground ?? "#4b5563"));
        _level.Foreground = Brushes.White;
        _levelBadge.MinWidth = _settings.UnitSkillBadgeShowLevelPrefix ? 44 : 26;
    }

    private void UpdateLevelText()
    {
        if (_skillModel is not null)
        {
            _level.Text = _skillModel.GetLevelText(_settings.UnitSkillBadgeShowLevelPrefix);
        }
    }
}

internal sealed class IconVisibilityConverter : IValueConverter
{
    public static IconVisibilityConverter Visible { get; } = new(false);
    public static IconVisibilityConverter Hidden { get; } = new(true);

    private readonly bool _invert;

    private IconVisibilityConverter(bool invert)
    {
        _invert = invert;
    }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool hasIcon = value is string iconPath && !string.IsNullOrWhiteSpace(iconPath);
        return _invert ? !hasIcon : hasIcon;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

internal sealed class IconAssetUriConverter : IValueConverter
{
    public static IconAssetUriConverter Instance { get; } = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string iconPath || string.IsNullOrWhiteSpace(iconPath))
        {
            return null;
        }

        if (iconPath.StartsWith("/Assets/", StringComparison.OrdinalIgnoreCase))
        {
            return $"avares://Odyssey{iconPath}";
        }

        if (iconPath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
        {
            return $"avares://Odyssey/{iconPath}";
        }

        return iconPath;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
