namespace DocPilot.Models;

/// <summary>
/// A single page of a parsed document.
/// </summary>
/// <param name="Number">1-based page index as shown in the UI footer.</param>
/// <param name="Text">Extracted plain text for this page.</param>
public sealed record DocumentPage(int Number, string Text);
