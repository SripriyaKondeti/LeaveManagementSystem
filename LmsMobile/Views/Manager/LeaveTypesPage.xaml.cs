using LmsMobile.ViewModels.Manager;

namespace LmsMobile.Views.Manager;

public partial class LeaveTypesPage : ContentPage
{
    private readonly LeaveTypesViewModel _vm;

    public LeaveTypesPage(LeaveTypesViewModel vm)
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