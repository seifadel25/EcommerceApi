public class Product
{
    public int? Id { get; set; } // EF Core will automatically configure this as the primary key
    public string? Category { get; set; }
    public string? ProductCode { get; set; } // Ensure this is unique via Fluent API or annotations
    public string? Name { get; set; }
    public string? ImagePath { get; set; } // Store the path of the image in local storage
    public decimal? Price { get; set; }
    public int? MinimumQuantity { get; set; }
    public double? DiscountRate { get; set; }
}