using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LmsMobile.Models;
using LmsMobile.Services;

namespace LmsMobile.ViewModels.Staff;

public partial class RequestLeaveViewModel : BaseViewModel
{
    private const int ReasonMaxLength = 500;

    private readonly ApiClient _api;
    private readonly SessionService _session;

    public ObservableCollection<LeaveType> LeaveTypes { get; } = new();
    public int ReasonLimit => ReasonMaxLength;

    [ObservableProperty, NotifyPropertyChangedFor(nameof(Today))]
    private DateTime currentDay = DateTime.Today;

    [ObservableProperty] private LeaveType? selectedLeaveType;

    [ObservableProperty, NotifyPropertyChangedFor(nameof(DayCount)), NotifyPropertyChangedFor(nameof(DayCountText))]
    private DateTime fromDate = DateTime.Today;

    [ObservableProperty, NotifyPropertyChangedFor(nameof(DayCount)), NotifyPropertyChangedFor(nameof(DayCountText))]
    private DateTime toDate = DateTime.Today;

    [ObservableProperty, NotifyPropertyChangedFor(nameof(ReasonCounter))]
    private string reason = "";

    public DateTime Today => CurrentDay.Date;

    // Calendar days, inclusive, weekends counted (same as the server).
    public int DayCount => Math.Max(0, (ToDate.Date - FromDate.Date).Days + 1);
    public string DayCountText => $"Days requested: {DayCount}";
    public string ReasonCounter => $"{Reason.Length}/{ReasonMaxLength}";

    public RequestLeaveViewModel(ApiClient api, SessionService session)
    {
        _api = api;
        _session = session;
    }

    partial void OnFromDateChanged(DateTime value)
    {
        if (ToDate.Date < value.Date) ToDate = value;
    }

    protected override async Task LoadCoreAsync()
    {
        CurrentDay = DateTime.Today;
        if (FromDate.Date < Today) FromDate = Today;
        if (ToDate.Date < FromDate.Date) ToDate = FromDate;

        var types = await _api.GetAsync<List<LeaveType>>("api/leavetypes");
        var previous = SelectedLeaveType?.LeaveTypeId;

        LeaveTypes.Clear();
        foreach (var t in types.Where(t => t.IsActive))
            LeaveTypes.Add(t);

        SelectedLeaveType = LeaveTypes.FirstOrDefault(t => t.LeaveTypeId == previous);
    }

    [RelayCommand]
    private Task SubmitAsync() => RunAsync(async () =>
    {
        var employeeId = _session.EmployeeId
            ?? throw new ApiException(0, "Your account is not linked to an employee record.");

        if (SelectedLeaveType is null) { ErrorMessage = "Choose a leave type."; return; }
        if (ToDate.Date < FromDate.Date) { ErrorMessage = "End date cannot be before start date."; return; }
        if (FromDate.Date < Today) { ErrorMessage = "Leave cannot start before today."; return; }
        if (string.IsNullOrWhiteSpace(Reason)) { ErrorMessage = "Enter a reason."; return; }

        await _api.PostAsync<LeaveRequest>("api/leaves", new CreateLeaveRequest
        {
            EmployeeId = employeeId,
            LeaveTypeId = SelectedLeaveType.LeaveTypeId,
            FromDate = FromDate.Date,
            ToDate = ToDate.Date,
            Reason = Reason.Trim()
        });

        Reason = "";
        FromDate = Today;
        ToDate = Today;
        SelectedLeaveType = null;

        await Shell.Current.DisplayAlertAsync("Submitted", "Your leave request is pending approval.", "OK");
        await Shell.Current.GoToAsync("//myrequests");
    });
}
