using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using DocPilot.Models;

namespace DocPilot.Converters;

/// <summary>
/// Maps <see cref="MessageRole"/> to a <see cref="HorizontalAlignment"/> so
/// user messages render on the right and assistant/system on the left.
/// </summary>
public sealed class MessageRoleToAlignmentConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is MessageRole role && role == MessageRole.User
            ? HorizontalAlignment.Right
            : HorizontalAlignment.Left;

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
