using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LmsMobile.Models;
using LmsMobile.Services;

namespace LmsMobile.ViewModels.Manager;

public partial class TeamViewModel : BaseViewModel
{
    private readonly ApiClient _api;
    private List<Employee> _all = new();

    public ObservableCollection<Employee> Employees { get; } = new();

    [ObservableProperty] private string searchText = "";

    public TeamViewModel(ApiClient api) => _api = api;

    protected override async Task LoadCoreAsync()
    {
        _all = await _api.GetAsync<List<Employee>>("api/employees");
        ApplyFilter();
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    private void ApplyFilter()
    {
        var q = SearchText.Trim();
        var matches = _all
            .Where(e => q.Length == 0
                || e.Name.Contains(q, StringComparison.OrdinalIgnoreCase)
                || e.EmployeeCode.Contains(q, StringComparison.OrdinalIgnoreCase)
                || e.Department.Contains(q, StringComparison.OrdinalIgnoreCase))
            .OrderBy(e => e.Name)
            .ToList();

        Employees.Clear();
        foreach (var e in matches)
            Employees.Add(e);
    }

    [RelayCommand]
    private Task OpenAsync(Employee employee) =>
        Shell.Current.GoToAsync($"employeedetail?id={employee.EmployeeId}");

    [RelayCommand]
    private Task AddAsync() => Shell.Current.GoToAsync("employeeedit?id=0");
}
