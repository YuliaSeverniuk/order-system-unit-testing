using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Moq;
using NUnit.Framework;
using OrderSystem.Controllers;
using OrderSystem.Data;
using OrderSystem.Models;
using OrderSystem.Services;

namespace OrderSystemTests
{
    [TestFixture]
    public class ProductControllerTests
    {
        private ProductDbContext _context = null!;
        private Mock<IProductService> _productServiceMock = null!;
        private ProductController _controller = null!;

        [SetUp]
        public void SetUp()
        {
            _context = TestDbContextFactory.CreateSeeded();
            _productServiceMock = new Mock<IProductService>();
            _controller = new ProductController(_context, _productServiceMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _controller.Dispose();
            _context.Dispose();
        }

        [Test]
        public void Index_WithSearchString_ReturnsProductsFromService()
        {
            var expected = new List<Product>
            {
                new Product { Id = 10, Name = "Laptop Pro", Price = 900M, CategoryId = 1 },
                new Product { Id = 11, Name = "Laptop Air", Price = 800M, CategoryId = 1 }
            };
            _productServiceMock
                .Setup(s => s.SearchProductsByName("Laptop"))
                .Returns(expected);

            var result = _controller.Index("Laptop");

            var view = result as ViewResult;
            Assert.That(view, Is.Not.Null);
            var model = view!.Model as IEnumerable<Product>;
            Assert.That(model, Is.EqualTo(expected));
            Assert.That(view.ViewData["SearchString"], Is.EqualTo("Laptop"));
            _productServiceMock.Verify(s => s.SearchProductsByName("Laptop"), Times.Once);
        }

        [Test]
        public void Index_WithoutSearchString_ReturnsAllProductsFromDatabase()
        {
            var result = _controller.Index(null!);

            var view = result as ViewResult;
            Assert.That(view, Is.Not.Null);
            var model = (view!.Model as IEnumerable<Product>)!.ToList();
            Assert.That(model, Has.Count.EqualTo(3));
            Assert.That(model.Select(p => p.Name), Is.EquivalentTo(new[] { "Laptop", "Smartphone", "Novel" }));
            Assert.That(model, Has.All.Matches<Product>(p => p.Category != null));
            _productServiceMock.Verify(s => s.SearchProductsByName(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void Create_Get_ReturnsViewWithCategoriesSelectList()
        {
            var result = _controller.Create();

            Assert.That(result, Is.InstanceOf<ViewResult>());
            var categories = _controller.ViewData["Categories"] as SelectList;
            Assert.That(categories, Is.Not.Null);
            Assert.That(categories!.Select(i => i.Text),
                Is.EquivalentTo(new[] { "Electronics", "Books", "Clothing" }));
        }

        [Test]
        public void Create_Post_ValidModel_AddsProductAndRedirectsToIndex()
        {
            var product = new Product { Name = "Tablet", Price = 299.99M, CategoryId = 1 };

            var result = _controller.Create(product);

            var redirect = result as RedirectToActionResult;
            Assert.That(redirect, Is.Not.Null);
            Assert.That(redirect!.ActionName, Is.EqualTo("Index"));
            Assert.That(_context.Products.Count(), Is.EqualTo(4));
            Assert.That(_context.Products.Any(p => p.Name == "Tablet"), Is.True);
        }

        [Test]
        public void Create_Post_InvalidModel_ReturnsViewWithSameModel()
        {
            var product = new Product { Name = null, Price = 10M, CategoryId = 1 };
            _controller.ModelState.AddModelError("Name", "The Name field is required.");

            var result = _controller.Create(product);

            var view = result as ViewResult;
            Assert.That(view, Is.Not.Null);
            Assert.That(view!.Model, Is.SameAs(product));
            Assert.That(_controller.ViewData["Categories"], Is.InstanceOf<SelectList>());
            Assert.That(_context.Products.Count(), Is.EqualTo(3));
        }

        [Test]
        public void Details_ExistingId_ReturnsViewWithProduct()
        {
            var result = _controller.Details(1);

            var view = result as ViewResult;
            Assert.That(view, Is.Not.Null);
            var product = view!.Model as Product;
            Assert.That(product, Is.Not.Null);
            Assert.That(product!.Id, Is.EqualTo(1));
            Assert.That(product.Name, Is.EqualTo("Laptop"));
            Assert.That(product.Category?.Name, Is.EqualTo("Electronics"));
        }

        [Test]
        public void Details_NonExistingId_ReturnsNotFound()
        {
            var result = _controller.Details(999);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }
    }
}
