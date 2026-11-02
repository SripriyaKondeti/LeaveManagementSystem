using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LmsMobile.Models;
using LmsMobile.Services;

namespace LmsMobile.ViewModels.Manager;

[QueryProperty(nameof(EmployeeId), "id")]
public partial class EmployeeEditViewModel : BaseViewModel
{
    private readonly ApiClient _api;

    [ObservableProperty, NotifyPropertyChangedFor(nameof(IsNew)), NotifyPropertyChangedFor(nameof(PageTitle)), NotifyPropertyChangedFor(nameof(CanDeactivate))]
    private int employeeId;

    [ObservableProperty] private string employeeCode = "";
    [ObservableProperty] private string name = "";
    [ObservableProperty] private string email = "";
    [ObservableProperty] private string department = "";
    [ObservableProperty] private DateTime joiningDate = DateTime.Today;
    [ObservableProperty] private bool isActive = true;
    [ObservableProperty] private string username = "";
    [ObservableProperty] private string password = "";
    [ObservableProperty] private string selectedRole = "Employee";

    [ObservableProperty, NotifyPropertyChangedFor(nameof(CanDeactivate))]
    private bool wasActive;

    public List<string> Roles { get; } = new() { "Employee", "Manager" };

    public bool IsNew => EmployeeId == 0;
    public string PageTitle => IsNew ? "Add Employee" : "Edit Employee";
    public bool CanDeactivate => !IsNew && WasActive;

    public EmployeeEditViewModel(ApiClient api) => _api = api;

    protected override async Task LoadCoreAsync()
    {
        if (IsNew) return;

        var e = await _api.GetAsync<Employee>($"api/employees/{EmployeeId}");
        EmployeeCode = e.EmployeeCode;
        Name = e.Name;
        Email = e.Email;
        Department = e.Department;
        JoiningDate = e.JoiningDate.Date;
        IsActive = e.IsActive;
        WasActive = e.IsActive;
    }

    [RelayCommand]
    private Task SaveAsync() => RunAsync(async () =>
    {
        if (string.IsNullOrWhiteSpace(EmployeeCode) || string.IsNullOrWhiteSpace(Name)
            || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Department))
        {
            ErrorMessage = "Code, name, email and department are required.";
            return;
        }

        if (!Email.Contains('@'))
        {
            ErrorMessage = "Enter a valid email address.";
            return;
        }

        if (IsNew)
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrEmpty(Password))
            {
                ErrorMessage = "Username and password are required.";
                return;
            }

            await _api.PostAsync<Employee>("api/employees", new CreateEmployeeRequest
            {
                EmployeeCode = EmployeeCode.Trim(),
                Name = Name.Trim(),
                Email = Email.Trim(),
                Department = Department.Trim(),
                JoiningDate = JoiningDate.Date,
                IsActive = IsActive,
                Username = Username.Trim(),
                Password = Password,
                Role = SelectedRole
            });
        }
        else
        {
            await _api.PutAsync<Employee>($"api/employees/{EmployeeId}", new Employee
            {
                EmployeeId = EmployeeId,
                EmployeeCode = EmployeeCode.Trim(),
                Name = Name.Trim(),
                Email = Email.Trim(),
                Department = Department.Trim(),
                JoiningDate = JoiningDate.Date,
                IsActive = IsActive
            });
        }

        Password = "";
        await Shell.Current.GoToAsync("..");
    });

    [RelayCommand]
    private async Task DeactivateAsync()
    {
        var ok = await Shell.Current.DisplayAlertAsync("Deactivate employee",
            $"Deactivate {Name}? They will no longer be able to sign in. You can reactivate them later by editing the employee.",
            "Deactivate", "Cancel");
        if (!ok) return;

        var succeeded = false;
        await RunAsync(async () =>
        {
            await _api.DeleteAsync($"api/employees/{EmployeeId}");
            succeeded = true;
        });

        if (succeeded)
            await Shell.Current.GoToAsync("..");
    }
}
