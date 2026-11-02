using LmsMobile.Services;
using LmsMobile.Views.Manager;
using LmsMobile.Views.Staff;

namespace LmsMobile;

public partial class AppShell : Shell
{
    private const string EmployeeBarRoute = "employee";
    private const string ManagerBarRoute = "manager";

    public AppShell()
    {
        InitializeComponent();

        // Pages opened from a tab.
        Routing.RegisterRoute("employeedetail", typeof(EmployeeDetailPage));
        Routing.RegisterRoute("employeeedit", typeof(EmployeeEditPage));
        Routing.RegisterRoute("leavetypeedit", typeof(LeaveTypeEditPage));
    }

    // Only the tabs of the signed-in role are ever created.
    public static async Task GoHomeAsync(SessionService session)
    {
        var shell = (AppShell)Current;
        shell.RemoveRoleTabs();
        shell.Items.Add(session.IsManager ? BuildManagerTabs() : BuildEmployeeTabs());
        await shell.GoToAsync(session.IsManager ? $"//{ManagerBarRoute}" : $"//{EmployeeBarRoute}");
    }

    public static async Task SignOutAsync()
    {
        var shell = (AppShell)Current;
        await shell.GoToAsync("//login");
        shell.RemoveRoleTabs();
    }

    public static async Task SessionExpiredAsync()
    {
        await SignOutAsync();
        await Current.DisplayAlertAsync("Session expired", "Please sign in again.", "OK");
    }

    private void RemoveRoleTabs()
    {
        foreach (var bar in Items.OfType<TabBar>().ToList())
            Items.Remove(bar);
    }

    private static TabBar BuildEmployeeTabs() => new()
    {
        Route = EmployeeBarRoute,
        Items =
        {
            Tab<EmployeeHomePage>("Home", "employeehome"),
            Tab<RequestLeavePage>("Request Leave", "requestleave"),
            Tab<MyRequestsPage>("My Requests", "myrequests"),
            Tab<ProfilePage>("Profile", "profile")
        }
    };

    private static TabBar BuildManagerTabs() => new()
    {
        Route = ManagerBarRoute,
        Items =
        {
            Tab<ManagerHomePage>("Home", "managerhome"),
            Tab<TeamPage>("Team", "team"),
            Tab<ApprovalsPage>("Approvals", "approvals"),
            Tab<LeaveTypesPage>("Leave Types", "leavetypes")
        }
    };

    private static ShellContent Tab<TPage>(string title, string route) where TPage : Page => new()
    {
        Title = title,
        Route = route,
        ContentTemplate = new DataTemplate(typeof(TPage))
    };
}
