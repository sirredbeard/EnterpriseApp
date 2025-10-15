using EnterpriseApp.Data;
using EnterpriseApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Drawing;

namespace EnterpriseApp.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _db;

    public ProductService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Product> CreateAsync(Product product)
    {
        // Add the product to the database
        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        // Create a 128x128 thumbnail
        using var bmp = new Bitmap(128, 128);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.AliceBlue);
        return product;
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        // Get all products
        return await _db.Products.AsNoTracking().ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        // Get a product by ID
        return await _db.Products.FindAsync(id);
    }
}
