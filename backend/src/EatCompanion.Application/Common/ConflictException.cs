namespace EatCompanion.Application.Common;

public class ConflictException : Exception
{
    public Guid? ResourceId { get; }

    public ConflictException(string message, Guid? resourceId = null) : base(message)
    {
        ResourceId = resourceId;
    }
}
