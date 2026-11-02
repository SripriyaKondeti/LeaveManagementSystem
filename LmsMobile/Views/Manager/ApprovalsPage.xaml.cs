using LmsMobile.ViewModels.Manager;

namespace LmsMobile.Views.Manager;

public partial class ApprovalsPage : ContentPage
{
    private readonly ApprovalsViewModel _vm;

    public ApprovalsPage(ApprovalsViewModel vm)
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