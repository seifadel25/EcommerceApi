public interface IAuthRepository
{
    Task<User> Authenticate(string username, string password);
    // You can also include methods for registration, password changes, etc.
}
