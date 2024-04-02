using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class UserRepository : IUserRepository
{
    private readonly EcommerceContext _context;
    private readonly PasswordHasher<User> _passwordHasher;

    public UserRepository(EcommerceContext context)
    {
        _context = context;
        _passwordHasher = new PasswordHasher<User>();

    }
    private async Task<bool> UsernameExists(string username)
    {
        return await _context.Users.AnyAsync(u => u.UserName == username);
    }

    private async Task<bool> EmailExists(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users.ToListAsync();
    }

    public async Task<User> GetByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<User> CreateAsync(User user)
    {
        if (await UsernameExists(user.UserName) || await EmailExists(user.Email))
        {
            throw new ArgumentException("Username or email already exists.");
        }

        user.Password = _passwordHasher.HashPassword(user, user.Password);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task UpdateAsync(User userToUpdate)
    {
        var existingUser = await _context.Users.FindAsync(userToUpdate.Id);
        if (existingUser == null)
        {
            throw new KeyNotFoundException($"User with ID {userToUpdate.Id} not found.");
        }

        if (!string.IsNullOrWhiteSpace(userToUpdate.Password))
        {
            existingUser.Password = _passwordHasher.HashPassword(existingUser, userToUpdate.Password);
        }
        if ((userToUpdate.UserName != existingUser.UserName && await UsernameExists(userToUpdate.UserName)) ||
              (userToUpdate.Email != existingUser.Email && await EmailExists(userToUpdate.Email)))
        {
            throw new ArgumentException("Username or email already exists.");
        }
        // Update properties
        existingUser.UserName = userToUpdate.UserName;
        // Ensure the password is hashed. This is just an example.
        // In a real application, you might want to handle password updates separately to avoid unintentional overwrites.
        existingUser.Password = userToUpdate.Password;
        existingUser.Email = userToUpdate.Email;
        existingUser.LastLoginTime = userToUpdate.LastLoginTime;

        // EF Core tracks changes to existingUser, so calling SaveChangesAsync will update it in the database.
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }
}
