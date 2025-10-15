using EnterpriseApp.Models;
using EnterpriseApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _svc;

    public ProductsController(IProductService svc)
    {
        // Inject the product service
        _svc = svc;
    }

    [HttpGet]
    // Get all products
    public async Task<IEnumerable<Product>> Get() => await _svc.GetAllAsync();

    [HttpGet("{id:int}")]
    // Get a product by ID
    public async Task<ActionResult<Product>> Get(int id)
    {
        var p = await _svc.GetByIdAsync(id);
        if (p == null) return NotFound();
        return p;
    }

    [HttpPost]
    // Create a new product
    public async Task<ActionResult<Product>> Post([FromBody] Product product)
    {
        var created = await _svc.CreateAsync(product);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }
}
