namespace Juris.Infrastructure.Identity;

public static class Roles
{
    public const string Admin = "Admin";
    public const string FirmHR = "FirmHR";
    public const string Subscriber = "Subscriber";

    public static readonly IReadOnlyList<string> All = new[] { Admin, FirmHR, Subscriber };
}
