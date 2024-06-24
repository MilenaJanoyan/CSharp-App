using ShopApp.Models;
using ShopApp.Repositories.Interfaces;
using ShopApp.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using static ShopApp.Enums.ProductEnums;

namespace ShopApp.Controllers;

/// <summary>
/// Controller for managing products.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _productRepository;
    private readonly IProductService _productService;

    public ProductsController(
        IProductRepository productRepository,
        IProductService productService)
    {
        _productRepository = productRepository;
        _productService = productService;
    }

    //TODO: Add advanced search

    //TODO: Add get for all the products

    //TODO: Add users, login and other realted logic

    /// <summary>
    /// Retrieves a list of products.
    /// </summary>
    /// <param name="skip">Number of products to skip for pagination.</param>
    /// <param name="take">Number of products to take for pagination.</param>
    /// <param name="searchTerm">Term to search products by name.</param>
    /// <returns>A list of products.</returns>
    [HttpGet]
    public async Task<ActionResult<List<Product>>> GetProducts(int skip = 0, int take = 10, string searchTerm = "")
    {
        var products = await _productRepository.GetAllProductsAsync();
        products = _productService.SearchProducts(products, searchTerm);
        var paginatedProducts = products.Skip(skip).Take(take).ToList();
        return Ok(paginatedProducts);
    }

    /// <summary>
    /// Retrieves a product by its ID.
    /// </summary>
    /// <param name="id">The product ID.</param>
    /// <returns>The product.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetProductById(Guid id)
    {
        var product = await _productRepository.GetProductByIdAsync(id);
        if (product == null)
        {
            return NotFound();
        }
        return Ok(product);
    }

    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="product">The product to create.</param>
    [HttpPost]
    public async Task<ActionResult<Product>> PostProduct(Product product)
    {
        product.Id = Guid.NewGuid();
        product.Status = ProductStatus.InStock;
        product.CreatedDate = DateTime.UtcNow;
        await _productRepository.AddProductAsync(product);
        return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
    }

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    /// <param name="id">The ID of the product to update.</param>
    /// <param name="product">The updated product object.</param>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(Guid id, Product product)
    {
        if (id != product.Id)
        {
            return BadRequest();
        }
        await _productRepository.UpdateProductAsync(product);
        return Ok(product);
    }

    /// <summary>
    /// Deletes a product by its ID.
    /// </summary>
    /// <param name="id">The ID of the product to delete.</param>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        var product = await _productRepository.GetProductByIdAsync(id);
        if (product == null)
        {
            return NotFound();
        }
        await _productRepository.DeleteProductAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Buys a specified quantity of a product.
    /// </summary>
    /// <param name="id">The ID of the product to buy.</param>
    /// <param name="quantity">The quantity to purchase.</param>
    /// <returns>ActionResult indicating success or failure.</returns>
    [HttpPost("{id}/buy")]
    public async Task<IActionResult> BuyProduct(Guid id, int quantity)
    {
        var product = await _productRepository.GetProductByIdAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        if (product.Status == ProductStatus.OutOfStock)
        {
            return BadRequest("Product is currently out of stock.");
        }

        if (quantity > product.StockQuantity)
        {
            return BadRequest($"Requested quantity ({quantity}) exceeds available stock ({product.StockQuantity}).");
        }

        bool purchaseSuccess = _productService.Buy(product.Id, quantity);

        if (!purchaseSuccess)
        {
            return BadRequest("Failed to purchase the product.");
        }

        return Ok("Product purchased successfully.");
    }
}
