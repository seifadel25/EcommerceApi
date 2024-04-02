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

    // POST: /Products
    // [HttpPost("upload")]
    // public async Task<IActionResult> UploadImage([FromForm] IFormFile image)
    // {
    //     if (image == null || image.Length == 0)
    //     {
    //         return BadRequest("Upload a file.");
    //     }

    //     // Define the path where the image will be saved
    //     // Ensure the 'images' directory exists under 'wwwroot'
    //     var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", image.FileName);

    //     // Create a new file stream where the image will be saved
    //     using (var stream = System.IO.File.Create(filePath))
    //     {
    //         await image.CopyToAsync(stream);
    //     }

    //     // Optionally save the file path or any other relevant data to your database here

    //     return Ok(new { message = "Image uploaded successfully!" });
    // }
    // [HttpPost]
    // //[Authorize] Will uncomment after adding Identity to the project
    // public async Task<ActionResult<Product>> CreateProduct(Product product)
    // {
    //     if (product == null)
    //     {
    //         return BadRequest("Product information is null");
    //     }

    //     var createdProduct = await _productRepository.CreateAsync(product);

    //     // Following RESTful principles, return a 201 status code with a 'Location' header
    //     // that contains the URL of the newly created product. Also, return the created entity.
    //     return CreatedAtAction(nameof(GetProduct), new { id = createdProduct.Id }, createdProduct);
    // }
    [HttpPost]
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
        var createdProduct = await _productRepository.CreateAsync(formData.Product);

        // Return the created product
        return CreatedAtAction(nameof(GetProduct), new { id = createdProduct.Id }, createdProduct);
    }


    // PUT: /Products/{id}

    [HttpPut("{id}")]

    //[Authorize] Will uncomment after adding Identity to the project

    public async Task<IActionResult> UpdateProduct(int id, Product product)
    {
        if (id != product.Id)
        {
            return BadRequest("Product ID mismatch");
        }

        var existingProduct = await _productRepository.GetByIdAsync(id);
        if (existingProduct == null)
        {
            return NotFound($"Product with ID {id} not found.");
        }

        await _productRepository.UpdateAsync(product);

        return NoContent(); // 204 No Content is typically returned when an update is successful.
    }

    // DELETE: /Products/{id}

    [HttpDelete("{id}")]
    //[Authorize] Will uncomment after adding Identity to the project

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
