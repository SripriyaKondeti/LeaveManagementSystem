using LmsMobile.ViewModels.Staff;

namespace LmsMobile.Views.Staff;

public partial class MyRequestsPage : ContentPage
{
    private readonly MyRequestsViewModel _vm;

    public MyRequestsPage(MyRequestsViewModel vm)
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
