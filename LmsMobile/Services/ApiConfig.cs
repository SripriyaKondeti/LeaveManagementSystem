namespace LmsMobile.Services;

public static class ApiConfig
{
    // Android emulator reaches the host PC through 10.0.2.2.
    // For a physical phone replace with "http://<your-PC-IP>:5245/".
    public static string BaseUrl =>
        DeviceInfo.Platform == DevicePlatform.Android
            ? "http://10.0.2.2:5245/"
            : "http://localhost:5245/";
}

public class ApiException : Exception
{
    public int StatusCode { get; }

    public ApiException(int statusCode, string message) : base(message)
    {
        StatusCode = statusCode;
    }
}
