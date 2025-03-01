namespace ExpenseTracker.Common.Abstractions.Identity
{
    public interface ICurrentUserService
    {
        string? UserId { get; }
        string? UserName { get; }
        string? Email { get; }
        bool IsAuthenticated { get; }
    }
}
