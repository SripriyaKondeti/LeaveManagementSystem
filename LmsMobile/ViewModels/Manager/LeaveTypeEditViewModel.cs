using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LmsMobile.Models;
using LmsMobile.Services;

namespace LmsMobile.ViewModels.Manager;

[QueryProperty(nameof(LeaveTypeId), "id")]
public partial class LeaveTypeEditViewModel : BaseViewModel
{
    private readonly ApiClient _api;
    private LeaveType? _loaded;

    [ObservableProperty, NotifyPropertyChangedFor(nameof(IsNew)), NotifyPropertyChangedFor(nameof(PageTitle))]
    private int leaveTypeId;

    [ObservableProperty] private string leaveTypeName = "";
    [ObservableProperty] private string maximumDays = "10";
    [ObservableProperty] private bool isActive = true;

    public bool IsNew => LeaveTypeId == 0;
    public string PageTitle => IsNew ? "Add Leave Type" : "Edit Leave Type";

    public LeaveTypeEditViewModel(ApiClient api) => _api = api;

    protected override async Task LoadCoreAsync()
    {
        if (IsNew) return;

        var t = await _api.GetAsync<LeaveType>($"api/leavetypes/{LeaveTypeId}");
        _loaded = t;
        LeaveTypeName = t.LeaveTypeName;
        MaximumDays = t.MaximumDays.ToString();
        IsActive = t.IsActive;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        ErrorMessage = null;

        if (string.IsNullOrWhiteSpace(LeaveTypeName))
        {
            ErrorMessage = "Name is required.";
            return;
        }

        if (!int.TryParse(MaximumDays, out var days) || days is < 1 or > 366)
        {
            ErrorMessage = "Maximum days must be a number from 1 to 366.";
            return;
        }

        // Editing an existing type rewrites employees' balances on the server.
        if (_loaded is not null && (_loaded.MaximumDays != days || _loaded.IsActive != IsActive))
        {
            var ok = await Shell.Current.DisplayAlertAsync("Update leave type",
                "Changing the maximum days or the active status updates every employee's balance for this type (deactivating resets balances to 0). Continue?",
                "Save", "Cancel");
            if (!ok) return;
        }

        var dto = new LeaveType
        {
            LeaveTypeId = LeaveTypeId,
            LeaveTypeName = LeaveTypeName.Trim(),
            MaximumDays = days,
            IsActive = IsActive
        };

        var saved = false;
        await RunAsync(async () =>
        {
            if (IsNew)
                await _api.PostAsync<LeaveType>("api/leavetypes", dto);
            else
                await _api.PutAsync<LeaveType>($"api/leavetypes/{LeaveTypeId}", dto);
            saved = true;
        });

        if (saved)
            await Shell.Current.GoToAsync("..");
    }
}
