﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Orders.Data;
using Orders.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Orders.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly OrdersDB db;
        private readonly IConfiguration configuration;

        public ProductsController(OrdersDB db, IConfiguration config)
        {
            this.db = db;   
            this.configuration = config;
            Console.WriteLine(config);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            var products = await db.Products.ToListAsync();
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            Product product = await db.Products.FindAsync(id);  
            if(product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        [HttpPost]
        public async Task<ActionResult<Product>> AddProduct(Product product)
        {
            var existing = await db.Products.FindAsync(product.Id);
            if (existing is not null) return Conflict();
            await db.Products.AddAsync(product);
            await db.SaveChangesAsync();
            return Created("Product added", product);
           
        }
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateProduct(int id, Product product)
        {
            var existing = await db.Products.FindAsync(id);
            if(existing is null) return NotFound();

            existing.Name = product.Name;
            existing.PriceDrebno = product.PriceDrebno;
            existing.PriceEdro = product.PriceEdro; 
            existing.Category = product.Category;
            existing.Code = product.Code;

            await db.SaveChangesAsync();

            return Ok(existing);

        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteProduct(int id)
        {
            var product = await db.Products.FindAsync(id);
            if (product is null)
            {
                return NotFound();
            }

            db.Products.Remove(product);
            await db.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("syncDatabase")]
        public async Task<ActionResult> SyncDatabase()
        {
            await ProductsLoader.SyncProductsData(db, configuration["Gensoft:ServerAddress"], configuration["Gensoft:DatabasePath"]);
            return Ok();
        }
    }
}
