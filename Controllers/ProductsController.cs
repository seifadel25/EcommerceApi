using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

[ApiController]
[Route("[controller]")]

public class ProductsController : ControllerBase
{
    private readonly IProductRepository _productRepository;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ProductsController(IProductRepository productRepository, IWebHostEnvironment webHostEnvironment)
    {
        _productRepository = productRepository;
        _webHostEnvironment = webHostEnvironment;

    }

    // GET: /Products
    // GET: /Products/Images/{imageName}
    [HttpGet("Images/{imageName}")]
    public IActionResult GetProductImage(string imageName)
    {
        // Define the path to the images directory
        var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", imageName);

        // Check if the image file exists
        if (!System.IO.File.Exists(imagePath))
        {
            return NotFound();
        }

        // Determine the image format based on the file extension
        var extension = Path.GetExtension(imageName)?.ToLower();
        string contentType;
        switch (extension)
        {
            case ".png":
                contentType = "image/png";
                break;
            case ".jpg":
            case ".jpeg":
                contentType = "image/jpeg";
                break;
            default:
                // Unsupported format, return 404 Not Found
                return NotFound();
        }

        // Return the image file
        var imageFileStream = System.IO.File.OpenRead(imagePath);
        return File(imageFileStream, contentType);
    }
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
    {
        var products = await _productRepository.GetAllAsync();
        return Ok(products);
    }

    // GET: /Products/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);

        if (product == null)
        {
            return NotFound();
        }

        return Ok(product);
    }
    [HttpPost]
    [Authorize] 
    public async Task<ActionResult<Product>> CreateProductWithImage([FromForm] ProductFormData formData)
    {
        // Validate the form data
        if (formData == null || formData.Image == null || formData.Product == null)
        {
            return BadRequest("Invalid form data.");
        }

        // Handle image upload
        if (formData.Image.Length == 0)
        {
            return BadRequest("Upload a file.");
        }

        // Define the path where the image will be saved
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", formData.Image.FileName);

        // Create a new file stream where the image will be saved
        using (var stream = System.IO.File.Create(filePath))
        {
            await formData.Image.CopyToAsync(stream);
        }

        // Optionally save the file path or any other relevant data to your database here

        // Create the product with the associated image path
        var product = formData.Product;
        product.ImagePath = filePath; // Save the image path in the product entity
        try
        {
            var createdProduct = await _productRepository.CreateAsync(formData.Product);
            // Return the created product
            return CreatedAtAction(nameof(GetProduct), new { id = createdProduct.Id }, createdProduct);
        }
        catch (ArgumentException ex)
        {
            // Handle the duplicate product code error
            return BadRequest("Product code already exists.");
        }
    }


    // PUT: /Products/{id}


    [Authorize] 

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromForm] ProductFormData formData)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (id != formData.Product.Id)
        {
            return BadRequest("Product ID mismatch");
        }

        var existingProduct = await _productRepository.GetByIdAsync(id);
        if (existingProduct == null)
        {
            return NotFound($"Product with ID {id} not found.");
        }

        // Check if an image file is included in the request
        if (formData.Image != null && formData.Image.Length > 0)
        {
            // Handle the image upload
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", formData.Image.FileName);
            formData.Product.ImagePath = filePath;

            // Create a new file stream where the image will be saved
            using (var stream = System.IO.File.Create(filePath))
            {
                await formData.Image.CopyToAsync(stream);
            }

            // Update the product entity's image path with the new image
            existingProduct.ImagePath = filePath;
        }
        else if (formData.Image == null)
        {
            // If no image is provided, retain the existing image path
            formData.Product.ImagePath = existingProduct.ImagePath;
        }
        // Update existing product details
        existingProduct.Name = formData.Product.Name;
        existingProduct.Price = formData.Product.Price;
        existingProduct.ImagePath = formData.Product.ImagePath;
        existingProduct.Category = formData.Product.Category;
        existingProduct.DiscountRate = formData.Product.DiscountRate;
        existingProduct.ProductCode = formData.Product.ProductCode;
        existingProduct.MinimumQuantity = formData.Product.MinimumQuantity;
        // Update additional fields as needed

        await _productRepository.UpdateAsync(existingProduct);

        return NoContent();
    }


    // DELETE: /Products/{id}

    [HttpDelete("{id}")]
    [Authorize] 

    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
        {
            return NotFound($"Product with ID {id} not found.");
        }

        await _productRepository.DeleteAsync(id);

        return NoContent(); // 204 No Content is the standard response for a successful DELETE operation.
    }
}
