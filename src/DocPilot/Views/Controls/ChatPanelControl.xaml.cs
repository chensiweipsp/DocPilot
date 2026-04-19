using System.Collections.Specialized;
using System.Windows.Controls;
using System.Windows.Input;
using DocPilot.ViewModels;

namespace DocPilot.Views.Controls;

/// <summary>
/// Chat panel (right pane). Handles Enter/Shift+Enter keyboard shortcuts and
/// keeps the transcript auto-scrolled to the newest message.
/// </summary>
public partial class ChatPanelControl : UserControl
{
    /// <summary>Create the chat panel and wire auto-scroll.</summary>
    public ChatPanelControl()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is ChatViewModel oldVm)
            oldVm.Messages.CollectionChanged -= OnMessagesChanged;
        if (e.NewValue is ChatViewModel newVm)
            newVm.Messages.CollectionChanged += OnMessagesChanged;
    }

    private void OnMessagesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Defer to layout pass so the new item exists before we scroll.
        Dispatcher.BeginInvoke(new System.Action(() => TranscriptScroll.ScrollToEnd()));
    }

    private void InputBox_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;

        var shift = (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;
        if (shift) return; // let the newline through

        if (DataContext is ChatViewModel vm && vm.SendCommand.CanExecute(null))
        {
            vm.SendCommand.Execute(null);
            e.Handled = true;
        }
    }
}
