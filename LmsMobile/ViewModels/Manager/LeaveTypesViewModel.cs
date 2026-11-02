using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using LmsMobile.Models;
using LmsMobile.Services;

namespace LmsMobile.ViewModels.Manager;

public partial class LeaveTypesViewModel : BaseViewModel
{
    private readonly ApiClient _api;

    public ObservableCollection<LeaveType> LeaveTypes { get; } = new();

    public LeaveTypesViewModel(ApiClient api) => _api = api;

    protected override async Task LoadCoreAsync()
    {
        var types = await _api.GetAsync<List<LeaveType>>("api/leavetypes");

        LeaveTypes.Clear();
        foreach (var t in types.OrderByDescending(t => t.IsActive).ThenBy(t => t.LeaveTypeName))
            LeaveTypes.Add(t);
    }

    [RelayCommand]
    private Task AddAsync() => Shell.Current.GoToAsync("leavetypeedit?id=0");

    [RelayCommand]
    private Task EditAsync(LeaveType type) => Shell.Current.GoToAsync($"leavetypeedit?id={type.LeaveTypeId}");

    [RelayCommand]
    private async Task DeactivateAsync(LeaveType type)
    {
        var ok = await Shell.Current.DisplayAlertAsync("Deactivate leave type",
            $"Deactivate '{type.LeaveTypeName}'? Every employee's balance for this type is reset to 0. You can reactivate it later by editing it.",
            "Deactivate", "Cancel");
        if (!ok) return;

        await RunAsync(() => _api.DeleteAsync($"api/leavetypes/{type.LeaveTypeId}"));

        var error = ErrorMessage;
        await LoadAsync();
        if (error is not null) ErrorMessage = error;
    }
}
