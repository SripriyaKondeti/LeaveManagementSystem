namespace LeaveManagementSystem.Swagger;

public static class SwaggerRoleConfiguration
{
    public static readonly HashSet<string> ManagerOnlyRoutes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "GET:/api/dashboard/leave-summary",

            "GET:/api/employees",
            "POST:/api/employees",
            "DELETE:/api/employees/{id}",

            "GET:/api/leavetypes",
            "GET:/api/leavetypes/{id}",
            "POST:/api/leavetypes",
            "PUT:/api/leavetypes/{id}",
            "DELETE:/api/leavetypes/{id}",

            "PUT:/api/leaves/{id}/approve",
            "PUT:/api/leaves/{id}/reject"
        };

    public static readonly HashSet<string> SharedRoutes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "GET:/api/employees/{id}",
            "PUT:/api/employees/{id}",

            "GET:/api/employees/{employeeId}/leave-balance",

            "POST:/api/leaves",
            "GET:/api/leaves",
            "GET:/api/leaves/{id}",
            "GET:/api/employees/{employeeId}/leaves"
        };

    public static readonly HashSet<string> PublicRoutes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "POST:/api/auth/login"
        };
}