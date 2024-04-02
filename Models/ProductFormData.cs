
using System.ComponentModel.DataAnnotations.Schema;

public class ProductFormData
{
    public Product Product { get; set; }
    [NotMapped]
    public IFormFile Image { get; set; }
}
