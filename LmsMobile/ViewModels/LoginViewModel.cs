using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LmsMobile.Models;
using LmsMobile.Services;

namespace LmsMobile.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly ApiClient _api;
    private readonly SessionService _session;

    [ObservableProperty] private string username = "";
    [ObservableProperty] private string password = "";

    public LoginViewModel(ApiClient api, SessionService session)
    {
        _api = api;
        _session = session;
    }

    // Called when the page appears: a saved, unexpired token skips Sign In.
    public async Task TryAutoLoginAsync()
    {
        if (await _session.RestoreAsync())
            await AppShell.GoHomeAsync(_session);
    }

    [RelayCommand]
    private Task SignInAsync() => RunAsync(async () =>
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Enter your username and password.";
            return;
        }

        var result = await _api.PostAsync<LoginResponse>("api/auth/login",
            new LoginRequest { Username = Username.Trim(), Password = Password });

        await _session.SaveAsync(result.Token, result.Role);
        Password = "";
        await AppShell.GoHomeAsync(_session);
    });
}
