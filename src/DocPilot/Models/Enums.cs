namespace DocPilot.Models;

/// <summary>
/// Identifies the role of a message in a chat exchange.
/// </summary>
public enum MessageRole
{
    /// <summary>A message authored by the human user.</summary>
    User,

    /// <summary>A message authored by the AI assistant.</summary>
    Assistant,

    /// <summary>A system or status message rendered inline with the transcript.</summary>
    System,

    /// <summary>An error notification rendered inline with the transcript.</summary>
    Error,
}

/// <summary>
/// Visual theme selection persisted with the user's settings.
/// </summary>
public enum ThemeMode
{
    /// <summary>Default dark palette.</summary>
    Dark,

    /// <summary>Light palette.</summary>
    Light,
}

/// <summary>
/// Logical document formats DocPilot knows how to parse.
/// </summary>
public enum DocumentType
{
    /// <summary>Unrecognised or unsupported extension.</summary>
    Unknown,

    /// <summary>Plain-text file (.txt, .md, .log …).</summary>
    Text,

    /// <summary>Portable Document Format (.pdf).</summary>
    Pdf,

    /// <summary>Office Open XML word-processing document (.docx).</summary>
    Docx,
}

/// <summary>
/// A quick-action preset attached to a chat button.
/// </summary>
public enum QuickAction
{
    /// <summary>Produce a concise summary of the loaded document.</summary>
    Summarize,

    /// <summary>Translate the loaded document to the target language.</summary>
    Translate,

    /// <summary>Offer suggested questions the user could ask.</summary>
    AskQuestions,
}
