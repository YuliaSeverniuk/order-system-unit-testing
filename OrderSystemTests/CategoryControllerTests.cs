using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using OrderSystem.Controllers;
using OrderSystem.Data;
using OrderSystem.Models;

namespace OrderSystemTests
{
    [TestFixture]
    public class CategoryControllerTests
    {
        private ProductDbContext _context = null!;
        private CategoryController _controller = null!;

        [SetUp]
        public void SetUp()
        {
            _context = TestDbContextFactory.Create();
            _context.Categories.AddRange(
                new Category { Id = 1, Name = "Electronics" },
                new Category { Id = 2, Name = "Books" },
                new Category { Id = 3, Name = "Clothing" });
            _context.Products.AddRange(
                new Product { Id = 1, Name = "Laptop", CategoryId = 1, Price = 999.99M },
                new Product { Id = 2, Name = "Smartphone", CategoryId = 1, Price = 599.99M },
                new Product { Id = 3, Name = "Novel", CategoryId = 2, Price = 14.99M });
            _context.SaveChanges();

            _controller = new CategoryController(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _controller.Dispose();
            _context.Dispose();
        }

        private static T GetProperty<T>(object item, string name) =>
            (T)item.GetType().GetProperty(name)!.GetValue(item)!;

        private List<object> GetModel(IActionResult result)
        {
            var view = result as ViewResult;
            Assert.That(view, Is.Not.Null);
            return ((System.Collections.IEnumerable)view!.Model!).Cast<object>().ToList();
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("name_asc")]
        public void Index_DefaultSortOrder_ReturnsCategoriesAscendingByName(string? sortOrder)
        {
            var model = GetModel(_controller.Index(sortOrder!));

            var names = model.Select(c => GetProperty<string>(c, "Name"));
            Assert.That(names, Is.EqualTo(new[] { "Books", "Clothing", "Electronics" }));
            Assert.That(_controller.ViewData["NameSortParam"], Is.EqualTo("name_desc"));
        }

        [Test]
        public void Index_NameDesc_ReturnsCategoriesDescendingByName()
        {
            var model = GetModel(_controller.Index("name_desc"));

            var names = model.Select(c => GetProperty<string>(c, "Name"));
            Assert.That(names, Is.EqualTo(new[] { "Electronics", "Clothing", "Books" }));
            Assert.That(_controller.ViewData["NameSortParam"], Is.EqualTo("name_asc"));
        }

        [Test]
        public void Index_ReturnsProductCountForEachCategory()
        {
            var model = GetModel(_controller.Index(null!));

            var counts = model.ToDictionary(
                c => GetProperty<string>(c, "Name"),
                c => GetProperty<int>(c, "ProductCount"));
            Assert.That(counts["Electronics"], Is.EqualTo(2));
            Assert.That(counts["Books"], Is.EqualTo(1));
            Assert.That(counts["Clothing"], Is.EqualTo(0));
        }

        [Test]
        public void Index_EmptyDatabase_ReturnsEmptyList()
        {
            using var emptyContext = TestDbContextFactory.Create();
            using var controller = new CategoryController(emptyContext);

            var model = GetModel(controller.Index(null!));

            Assert.That(model, Is.Empty);
        }
    }
}
