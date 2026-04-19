using CommunityToolkit.Mvvm.ComponentModel;

namespace DocPilot.ViewModels;

/// <summary>
/// Root base class for all view-models. Currently just re-exports
/// <see cref="ObservableObject"/>; kept as an explicit type so cross-cutting
/// behaviour (e.g. busy state) can be added in one place later.
/// </summary>
public abstract class ViewModelBase : ObservableObject
{
}
