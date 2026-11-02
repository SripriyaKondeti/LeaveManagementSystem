using LmsMobile.ViewModels.Manager;

namespace LmsMobile.Views.Manager;

public partial class EmployeeEditPage : ContentPage
{
    private readonly EmployeeEditViewModel _vm;

    public EmployeeEditPage(EmployeeEditViewModel vm)
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