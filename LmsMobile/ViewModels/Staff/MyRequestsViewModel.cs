using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using LmsMobile.Models;
using LmsMobile.Services;

namespace LmsMobile.ViewModels.Staff;

public partial class MyRequestsViewModel : BaseViewModel
{
    private readonly ApiClient _api;

    public ObservableCollection<LeaveRequest> Requests { get; } = new();
    public List<string> Filters { get; } = new() { "All", "Pending", "Approved", "Rejected" };

    [ObservableProperty] private string selectedFilter = "All";

    public MyRequestsViewModel(ApiClient api) => _api = api;

    partial void OnSelectedFilterChanged(string value) => _ = LoadAsync();

    protected override async Task LoadCoreAsync()
    {
        var url = SelectedFilter == "All" ? "api/leaves" : $"api/leaves?status={SelectedFilter}";
        var leavesTask = _api.GetAsync<List<LeaveRequest>>(url);
        var typesTask = _api.GetAsync<List<LeaveType>>("api/leavetypes");
        await Task.WhenAll(leavesTask, typesTask);

        var typeNames = typesTask.Result.ToDictionary(t => t.LeaveTypeId, t => t.LeaveTypeName);

        Requests.Clear();
        foreach (var l in leavesTask.Result.OrderByDescending(l => l.AppliedDate))
        {
            l.LeaveTypeName = typeNames.GetValueOrDefault(l.LeaveTypeId, "Leave");
            Requests.Add(l);
        }
    }
}
