using LmsMobile.ViewModels.Manager;

namespace LmsMobile.Views.Manager;

public partial class ManagerHomePage : ContentPage
{
    private readonly ManagerHomeViewModel _vm;

    public ManagerHomePage(ManagerHomeViewModel vm)
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