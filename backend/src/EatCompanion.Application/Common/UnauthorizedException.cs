namespace EatCompanion.Application.Common;

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message = "Invalid email or password.") : base(message) { }
}
