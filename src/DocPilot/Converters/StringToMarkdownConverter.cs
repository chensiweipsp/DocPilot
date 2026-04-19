using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Documents;
using Markdig;

namespace DocPilot.Converters;

/// <summary>
/// Converts a Markdown string into a <see cref="FlowDocument"/> rendered by
/// <c>Markdig.Wpf</c>. Used by the assistant message bubble.
/// </summary>
public sealed class StringToMarkdownConverter : IValueConverter
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();

    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var text = value as string ?? string.Empty;
        if (string.IsNullOrEmpty(text))
            return new FlowDocument();

        try
        {
            return Markdig.Wpf.Markdown.ToFlowDocument(text, Pipeline);
        }
        catch
        {
            var doc = new FlowDocument();
            doc.Blocks.Add(new Paragraph(new Run(text)));
            return doc;
        }
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
