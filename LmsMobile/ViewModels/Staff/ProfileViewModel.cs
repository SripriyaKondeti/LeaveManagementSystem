using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LmsMobile.Models;
using LmsMobile.Services;

namespace LmsMobile.ViewModels.Staff;

public partial class ProfileViewModel : BaseViewModel
{
    private readonly ApiClient _api;
    private readonly SessionService _session;

    [ObservableProperty] private Employee? profile;
    [ObservableProperty] private string name = "";
    [ObservableProperty] private string email = "";
    [ObservableProperty] private string department = "";

    public ProfileViewModel(ApiClient api, SessionService session)
    {
        _api = api;
        _session = session;
    }

    protected override async Task LoadCoreAsync()
    {
        var id = _session.EmployeeId
            ?? throw new ApiException(0, "Your account is not linked to an employee record.");

        var me = await _api.GetAsync<Employee>($"api/employees/{id}");
        Apply(me);
    }

    [RelayCommand]
    private Task SaveAsync() => RunAsync(async () =>
    {
        if (Profile is null) return;

        if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Department))
        {
            ErrorMessage = "Name, email and department are required.";
            return;
        }

        if (!Email.Contains('@'))
        {
            ErrorMessage = "Enter a valid email address.";
            return;
        }

        // Code, joining date and status are sent back unchanged (employees cannot change IsActive).
        var update = new Employee
        {
            EmployeeId = Profile.EmployeeId,
            EmployeeCode = Profile.EmployeeCode,
            JoiningDate = Profile.JoiningDate,
            IsActive = Profile.IsActive,
            Name = Name.Trim(),
            Email = Email.Trim(),
            Department = Department.Trim()
        };

        var saved = await _api.PutAsync<Employee>($"api/employees/{update.EmployeeId}", update);
        Apply(saved);
        await Shell.Current.DisplayAlertAsync("Saved", "Your details were updated.", "OK");
    });

    [RelayCommand]
    private async Task SignOutAsync()
    {
        _session.Clear();
        await AppShell.SignOutAsync();
    }

    private void Apply(Employee e)
    {
        Profile = e;
        Name = e.Name;
        Email = e.Email;
        Department = e.Department;
    }
}
