using Microsoft.EntityFrameworkCore;
using OrderSystem.Data;
using OrderSystem.Models;

namespace OrderSystemTests
{
    internal static class TestDbContextFactory
    {
        public static ProductDbContext Create()
        {
            var options = new DbContextOptionsBuilder<ProductDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new ProductDbContext(options);
        }

        public static ProductDbContext CreateSeeded()
        {
            var context = Create();

            var electronics = new Category { Id = 1, Name = "Electronics" };
            var books = new Category { Id = 2, Name = "Books" };
            var clothing = new Category { Id = 3, Name = "Clothing" };
            context.Categories.AddRange(electronics, books, clothing);

            context.Products.AddRange(
                new Product { Id = 1, Name = "Laptop", CategoryId = 1, Price = 999.99M },
                new Product { Id = 2, Name = "Smartphone", CategoryId = 1, Price = 599.99M },
                new Product { Id = 3, Name = "Novel", CategoryId = 2, Price = 14.99M });

            context.SaveChanges();
            context.ChangeTracker.Clear();
            return context;
        }
    }
}
