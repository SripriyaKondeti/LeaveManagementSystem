using LmsMobile.ViewModels;

namespace LmsMobile.Views;

public partial class LoginPage : ContentPage
{
    private readonly LoginViewModel _vm;

    public LoginPage(LoginViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;

        // Loaded fires after the Shell has finished building, so navigating from here cannot collide with startup.
        Loaded += async (_, _) => await _vm.TryAutoLoginAsync();
    }
}
