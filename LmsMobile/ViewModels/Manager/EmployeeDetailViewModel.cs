using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LmsMobile.Models;
using LmsMobile.Services;

namespace LmsMobile.ViewModels.Manager;

[QueryProperty(nameof(EmployeeId), "id")]
public partial class EmployeeDetailViewModel : BaseViewModel
{
    private readonly ApiClient _api;

    [ObservableProperty] private int employeeId;
    [ObservableProperty] private Employee? employee;
    [ObservableProperty, NotifyPropertyChangedFor(nameof(HasBalanceNote))] private string? balanceNote;

    public bool HasBalanceNote => !string.IsNullOrEmpty(BalanceNote);

    public ObservableCollection<LeaveBalance> Balances { get; } = new();
    public ObservableCollection<LeaveRequest> History { get; } = new();

    public EmployeeDetailViewModel(ApiClient api) => _api = api;

    protected override async Task LoadCoreAsync()
    {
        if (EmployeeId <= 0) return;

        var employeeTask = _api.GetAsync<Employee>($"api/employees/{EmployeeId}");
        var historyTask = _api.GetAsync<List<LeaveRequest>>($"api/employees/{EmployeeId}/leaves");
        var typesTask = _api.GetAsync<List<LeaveType>>("api/leavetypes");
        // The server refuses balances for inactive employees, so this call is allowed to fail on its own.
        var balanceTask = LoadBalancesAsync();
        await Task.WhenAll(employeeTask, historyTask, typesTask, balanceTask);

        Employee = employeeTask.Result;

        var typeNames = typesTask.Result.ToDictionary(t => t.LeaveTypeId, t => t.LeaveTypeName);
        History.Clear();
        foreach (var l in historyTask.Result.OrderByDescending(l => l.AppliedDate))
        {
            l.LeaveTypeName = typeNames.GetValueOrDefault(l.LeaveTypeId, "Leave");
            History.Add(l);
        }
    }

    private async Task LoadBalancesAsync()
    {
        Balances.Clear();
        BalanceNote = null;
        try
        {
            var balances = await _api.GetAsync<List<LeaveBalance>>($"api/employees/{EmployeeId}/leave-balance");
            foreach (var b in balances.Where(b => b.TotalDays > 0))
                Balances.Add(b);
            if (Balances.Count == 0) BalanceNote = "No leave balances.";
        }
        catch (ApiException ex) when (ex.StatusCode == 409)
        {
            BalanceNote = ex.Message;
        }
    }

    [RelayCommand]
    private Task EditAsync() => Shell.Current.GoToAsync($"employeeedit?id={EmployeeId}");
}
