using LmsMobile.ViewModels.Staff;

namespace LmsMobile.Views.Staff;

public partial class RequestLeavePage : ContentPage
{
    private readonly RequestLeaveViewModel _vm;

    public RequestLeavePage(RequestLeaveViewModel vm)
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
