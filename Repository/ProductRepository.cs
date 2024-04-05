using Microsoft.EntityFrameworkCore;

public class ProductRepository : IProductRepository
{
    private readonly EcommerceContext _context;
    private async Task<bool> CodeExist(string? code)
    {
        return await _context.Products.AnyAsync(u => u.ProductCode == code);
    }
    public ProductRepository(EcommerceContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _context.Products.ToListAsync();
    }

    public async Task<Product> GetByIdAsync(int id)
    {
        return await _context.Products.FindAsync(id);
    }

    public async Task<Product> CreateAsync(Product product)
    {
        if (await CodeExist(product.ProductCode))
        {
            throw new ArgumentException("Product with this code already exists.");
        }
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task UpdateAsync(Product productToUpdate)
    {
        var existingProduct = await _context.Products.FindAsync(productToUpdate.Id);
        if (existingProduct == null)
        {
            throw new KeyNotFoundException($"Product with ID {productToUpdate.Id} not found.");
        }


        // Update properties
        existingProduct.Category = productToUpdate.Category;
        existingProduct.ProductCode = productToUpdate.ProductCode;
        existingProduct.Name = productToUpdate.Name;
        existingProduct.ImagePath = productToUpdate.ImagePath;
        existingProduct.Price = productToUpdate.Price;
        existingProduct.MinimumQuantity = productToUpdate.MinimumQuantity;
        existingProduct.DiscountRate = productToUpdate.DiscountRate;

        // EF Core tracks changes to existingProduct, so calling SaveChangesAsync will update it in the database.
        await _context.SaveChangesAsync();
    }


    public async Task DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
    }
}
