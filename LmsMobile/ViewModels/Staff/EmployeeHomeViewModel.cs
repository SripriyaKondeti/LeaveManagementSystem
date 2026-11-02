using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LmsMobile.Models;
using LmsMobile.Services;

namespace LmsMobile.ViewModels.Staff;

public partial class EmployeeHomeViewModel : BaseViewModel
{
    private readonly ApiClient _api;
    private readonly SessionService _session;

    [ObservableProperty] private string greeting = "";
    [ObservableProperty] private int pendingCount;

    public ObservableCollection<LeaveBalance> Balances { get; } = new();
    public bool NoBalances => Balances.Count == 0;

    public EmployeeHomeViewModel(ApiClient api, SessionService session)
    {
        _api = api;
        _session = session;
    }

    protected override async Task LoadCoreAsync()
    {
        var id = _session.EmployeeId
            ?? throw new ApiException(0, "Your account is not linked to an employee record.");

        var meTask = _api.GetAsync<Employee>($"api/employees/{id}");
        var balanceTask = _api.GetAsync<List<LeaveBalance>>($"api/employees/{id}/leave-balance");
        var pendingTask = _api.GetAsync<List<LeaveRequest>>("api/leaves?status=Pending");
        await Task.WhenAll(meTask, balanceTask, pendingTask);

        Greeting = $"Hello, {meTask.Result.Name}";
        PendingCount = pendingTask.Result.Count;

        Balances.Clear();
        // Inactive leave types come back as 0 / 0.
        foreach (var b in balanceTask.Result.Where(b => b.TotalDays > 0))
            Balances.Add(b);

        OnPropertyChanged(nameof(NoBalances));
    }

    [RelayCommand]
    private Task RequestLeaveAsync() => Shell.Current.GoToAsync("//requestleave");
}
