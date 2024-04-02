using System.ComponentModel.DataAnnotations;


public class User
{
    public int Id { get; set; } // EF Core will automatically configure this as the primary key

    [Required]
    [StringLength(50, MinimumLength = 5)]

    public string UserName { get; set; } // Ensure this is unique
    [Required]
    [StringLength(100, MinimumLength = 6)]
    [RegularExpression("(?=.*[0-9])(?=.*[a-zA-Z])(.){6,}", ErrorMessage = "Password must contain letters and numbers and be at least 6 characters long.")]


    public string Password { get; set; } // Consider hashing this password
    [Required]
    [EmailAddress]
    public string Email { get; set; } // Ensure this is unique
    public DateTime LastLoginTime { get; set; }
}