using LmsMobile.ViewModels.Manager;

namespace LmsMobile.Views.Manager;

public partial class LeaveTypeEditPage : ContentPage
{
    private readonly LeaveTypeEditViewModel _vm;

    public LeaveTypeEditPage(LeaveTypeEditViewModel vm)
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