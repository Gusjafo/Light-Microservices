namespace OrderService.External
{
    public interface IUserServiceClient
    {
        Task<bool> UserExistsAsync(Guid userId, CancellationToken ct);
    }
}
