using System.Linq;
using System.Windows;
using DocPilot.ViewModels;

namespace DocPilot.Views;

/// <summary>Root shell window. Accepts drag-and-drop file input.</summary>
public partial class MainWindow : Window
{
    /// <summary>Create the main window.</summary>
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop)
            ? DragDropEffects.Copy
            : DragDropEffects.None;
        e.Handled = true;
    }

    private async void Window_Drop(object sender, DragEventArgs e)
    {
        if (DataContext is not MainViewModel vm) return;
        if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

        var paths = (string[])e.Data.GetData(DataFormats.FileDrop);
        var first = paths?.FirstOrDefault();
        if (string.IsNullOrEmpty(first)) return;

        await vm.LoadDocumentAsync(first);
    }
}
