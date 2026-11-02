using LmsMobile.ViewModels.Manager;

namespace LmsMobile.Views.Manager;

public partial class EmployeeDetailPage : ContentPage
{
    private readonly EmployeeDetailViewModel _vm;

    public EmployeeDetailPage(EmployeeDetailViewModel vm)
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