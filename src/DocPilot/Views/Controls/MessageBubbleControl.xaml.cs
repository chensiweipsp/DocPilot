using System.Windows.Controls;

namespace DocPilot.Views.Controls;

/// <summary>Single message bubble; chooses plain or Markdown rendering by role.</summary>
public partial class MessageBubbleControl : UserControl
{
    /// <summary>Create the bubble.</summary>
    public MessageBubbleControl()
    {
        InitializeComponent();
    }
}
