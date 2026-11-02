using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using LmsMobile.Models;
using LmsMobile.Services;

namespace LmsMobile.ViewModels.Manager;

public partial class ApprovalsViewModel : BaseViewModel
{
    private readonly ApiClient _api;

    public ObservableCollection<LeaveRequest> Pending { get; } = new();

    public ApprovalsViewModel(ApiClient api) => _api = api;

    protected override async Task LoadCoreAsync()
    {
        var leavesTask = _api.GetAsync<List<LeaveRequest>>("api/leaves?status=Pending");
        var employeesTask = _api.GetAsync<List<Employee>>("api/employees");
        var typesTask = _api.GetAsync<List<LeaveType>>("api/leavetypes");
        await Task.WhenAll(leavesTask, employeesTask, typesTask);

        var names = employeesTask.Result.ToDictionary(e => e.EmployeeId, e => e.Name);
        var types = typesTask.Result.ToDictionary(t => t.LeaveTypeId, t => t.LeaveTypeName);

        Pending.Clear();
        foreach (var l in leavesTask.Result.OrderBy(l => l.FromDate))
        {
            l.EmployeeName = names.GetValueOrDefault(l.EmployeeId, $"Employee #{l.EmployeeId}");
            l.LeaveTypeName = types.GetValueOrDefault(l.LeaveTypeId, "Leave");
            Pending.Add(l);
        }
    }

    [RelayCommand] private Task ApproveAsync(LeaveRequest request) => DecideAsync(request, "approve");
    [RelayCommand] private Task RejectAsync(LeaveRequest request) => DecideAsync(request, "reject");

    private async Task DecideAsync(LeaveRequest request, string action)
    {
        var approving = action == "approve";
        var comments = await Shell.Current.DisplayPromptAsync(
            approving ? "Approve leave" : "Reject leave",
            $"{request.EmployeeName}: {request.LeaveTypeName}, {request.DateRange}",
            approving ? "Approve" : "Reject",
            "Cancel",
            approving ? "Comments (optional)" : "Reason for rejection (optional)");
        if (comments is null) return; // Cancel tapped

        await RunAsync(() => _api.PutAsync<LeaveRequest>(
            $"api/leaves/{request.LeaveRequestId}/{action}",
            new LeaveAction { Comments = string.IsNullOrWhiteSpace(comments) ? null : comments.Trim() }));

        // Reload either way: on 409 another manager may already have decided this request.
        var error = ErrorMessage;
        await LoadAsync();
        if (error is not null) ErrorMessage = error;
    }
}
