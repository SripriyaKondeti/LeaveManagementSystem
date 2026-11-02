using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LmsMobile.Models;
using LmsMobile.Services;

namespace LmsMobile.ViewModels.Manager;

public partial class ManagerHomeViewModel : BaseViewModel
{
    private readonly ApiClient _api;
    private readonly SessionService _session;

    [ObservableProperty] private LeaveSummary? summary;

    public ManagerHomeViewModel(ApiClient api, SessionService session)
    {
        _api = api;
        _session = session;
    }

    protected override async Task LoadCoreAsync() =>
        Summary = await _api.GetAsync<LeaveSummary>("api/dashboard/leave-summary");

    [RelayCommand] private Task OpenApprovalsAsync() => Shell.Current.GoToAsync("//approvals");
    [RelayCommand] private Task OpenTeamAsync() => Shell.Current.GoToAsync("//team");

    [RelayCommand]
    private async Task SignOutAsync()
    {
        _session.Clear();
        await AppShell.SignOutAsync();
    }
}
