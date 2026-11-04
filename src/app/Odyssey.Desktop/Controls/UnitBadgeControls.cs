using System;
using System.Globalization;
using AsyncImageLoader;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Odyssey.ViewModels.Tools;

namespace Odyssey.Controls;

public sealed class UnitItemBadgeControl : UserControl
{
    private const double BadgeSize = 46;
    private readonly Viewbox _viewbox;
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
            Stretch = Avalonia.Media.Stretch.Uniform,
        };
        image.Bind(AdvancedImage.SourceProperty, new Binding(nameof(UnitItemViewModel.IconPath))
        {
            Converter = IconAssetUriConverter.Instance,
        });
        image.Bind(Visual.IsVisibleProperty, new Binding(nameof(UnitItemViewModel.IconPath))
        {
            Converter = IconVisibilityConverter.Visible,
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
            FontSize = 11,
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
            CornerRadius = new CornerRadius(4),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom,
            Margin = new Thickness(0, 2, 0, 0),
            Padding = new Thickness(3, 0),
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
        grid.Children.Add(itemName);
        grid.Children.Add(quantityBadge);

        var badge = new Border
        {
            Width = BadgeSize,
            Height = BadgeSize,
            CornerRadius = new CornerRadius(8),
            BorderBrush = new SolidColorBrush(Color.Parse("#8f6d42")),
            BorderThickness = new Thickness(1.2),
            Background = new SolidColorBrush(Color.Parse("#d9d9d9")),
            Child = grid,
        };
        badge.Bind(ToolTip.TipProperty, new Binding(nameof(UnitItemViewModel.TooltipLabel)));

        _viewbox = new Viewbox
        {
            Width = BadgeSize,
            Height = BadgeSize,
            Stretch = Stretch.Uniform,
            Child = badge,
        };
        Content = _viewbox;
    }
}

public sealed class UnitSkillBadgeControl : UserControl
{
    private const double BadgeSize = 46;
    private readonly Viewbox _viewbox;
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
        var changeOverlay = new Border { CornerRadius = new CornerRadius(23) };
        changeOverlay.Bind(Border.BackgroundProperty, new Binding(nameof(UnitSkillViewModel.ChangeCircleOverlay)));

        var image = new AdvancedImage((Uri)null!)
        {
            Width = 24,
            Height = 24,
            Margin = new Thickness(0, 0, 0, 8),
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
            Margin = new Thickness(4, 4, 4, 12),
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

        var level = new TextBlock
        {
            FontSize = 11,
            FontWeight = FontWeight.Black,
            Foreground = Brushes.White,
            TextAlignment = TextAlignment.Center,
        };
        level.Bind(TextBlock.TextProperty, new Binding(nameof(UnitSkillViewModel.LevelText)));

        var levelBadge = new Border
        {
            CornerRadius = new CornerRadius(8),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom,
            Margin = new Thickness(0, 0, 0, 2),
            Padding = new Thickness(5, 0),
            Child = level,
        };
        levelBadge.Bind(Border.BackgroundProperty, new Binding(nameof(UnitSkillViewModel.LevelBackground)));

        var grid = new Grid();
        grid.Children.Add(changeOverlay);
        grid.Children.Add(image);
        grid.Children.Add(skillName);
        grid.Children.Add(levelBadge);

        var badge = new Border
        {
            Width = BadgeSize,
            Height = BadgeSize,
            CornerRadius = new CornerRadius(23),
            BorderThickness = new Thickness(4),
            Background = new SolidColorBrush(Color.Parse("#cfe6f0")),
            Child = grid,
        };
        badge.Bind(Border.BorderBrushProperty, new Binding(nameof(UnitSkillViewModel.ChangeBorderBrush)));
        badge.Bind(Border.BorderThicknessProperty, new Binding(nameof(UnitSkillViewModel.ChangeBorderThickness)));
        badge.Bind(ToolTip.TipProperty, new Binding(nameof(UnitSkillViewModel.TooltipLabel)));

        _viewbox = new Viewbox
        {
            Width = BadgeSize,
            Height = BadgeSize,
            Stretch = Stretch.Uniform,
            Child = badge,
        };
        Content = _viewbox;
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
