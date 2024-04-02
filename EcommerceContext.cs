using Microsoft.EntityFrameworkCore;

public class EcommerceContext : DbContext
{
    public EcommerceContext(DbContextOptions<EcommerceContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; }
    
    public DbSet<User> Users { get; set; }
}