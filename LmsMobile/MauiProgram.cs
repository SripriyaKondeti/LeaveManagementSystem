using Microsoft.Extensions.Logging;
using LmsMobile.Services;
using LmsMobile.ViewModels;
using LmsMobile.ViewModels.Manager;
using LmsMobile.ViewModels.Staff;
using LmsMobile.Views;
using LmsMobile.Views.Manager;
using LmsMobile.Views.Staff;

namespace LmsMobile;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		builder.Services.AddSingleton<SessionService>();
		builder.Services.AddTransient<AuthHeaderHandler>();
		builder.Services.AddHttpClient<ApiClient>(c =>
			{
				c.BaseAddress = new Uri(ApiConfig.BaseUrl);
				c.Timeout = TimeSpan.FromSeconds(20);
			})
			.AddHttpMessageHandler<AuthHeaderHandler>();

		builder.Services.AddTransient<LoginPage>();
		builder.Services.AddTransient<LoginViewModel>();

		builder.Services.AddTransient<EmployeeHomePage>();
		builder.Services.AddTransient<EmployeeHomeViewModel>();
		builder.Services.AddTransient<RequestLeavePage>();
		builder.Services.AddTransient<RequestLeaveViewModel>();
		builder.Services.AddTransient<MyRequestsPage>();
		builder.Services.AddTransient<MyRequestsViewModel>();
		builder.Services.AddTransient<ProfilePage>();
		builder.Services.AddTransient<ProfileViewModel>();

		builder.Services.AddTransient<ManagerHomePage>();
		builder.Services.AddTransient<ManagerHomeViewModel>();
		builder.Services.AddTransient<TeamPage>();
		builder.Services.AddTransient<TeamViewModel>();
		builder.Services.AddTransient<EmployeeDetailPage>();
		builder.Services.AddTransient<EmployeeDetailViewModel>();
		builder.Services.AddTransient<EmployeeEditPage>();
		builder.Services.AddTransient<EmployeeEditViewModel>();
		builder.Services.AddTransient<ApprovalsPage>();
		builder.Services.AddTransient<ApprovalsViewModel>();
		builder.Services.AddTransient<LeaveTypesPage>();
		builder.Services.AddTransient<LeaveTypesViewModel>();
		builder.Services.AddTransient<LeaveTypeEditPage>();
		builder.Services.AddTransient<LeaveTypeEditViewModel>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
