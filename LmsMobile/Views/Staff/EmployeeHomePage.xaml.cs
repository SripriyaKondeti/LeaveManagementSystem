using LmsMobile.ViewModels.Staff;

namespace LmsMobile.Views.Staff;

public partial class EmployeeHomePage : ContentPage
{
    private readonly EmployeeHomeViewModel _vm;

    public EmployeeHomePage(EmployeeHomeViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
    }
}
