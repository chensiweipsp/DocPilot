using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using DocPilot.Models;

namespace DocPilot.Converters;

/// <summary>
/// Maps <see cref="MessageRole"/> to its bubble background brush.
/// Looks up the brush via <see cref="Application.Current"/> so theme swaps
/// are picked up automatically.
/// </summary>
public sealed class MessageRoleToBrushConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var key = value switch
        {
            MessageRole.User => "Accent.UserGradient",
            MessageRole.Assistant => "Accent.AssistantGradient",
            MessageRole.Error => "Accent.Error",
            _ => "Palette.Surface",
        };
        var brush = Application.Current?.TryFindResource(key) as Brush;
        return brush ?? (Brush)new SolidColorBrush(Colors.Gray);
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
