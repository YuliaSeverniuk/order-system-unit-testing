using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using OrderSystem.Controllers;
using OrderSystem.Data;
using OrderSystem.Models;

namespace OrderSystemTests
{
    [TestFixture]
    public class OrderControllerTests
    {
        private ProductDbContext _context = null!;
        private OrderController _controller = null!;

        [SetUp]
        public void SetUp()
        {
            _context = TestDbContextFactory.CreateSeeded();
            _controller = new OrderController(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _controller.Dispose();
            _context.Dispose();
        }

        [Test]
        public void Create_Get_ReturnsViewWithProductsDropdown()
        {
            var result = _controller.Create();

            Assert.That(result, Is.InstanceOf<ViewResult>());
            var products = _controller.ViewData["Products"] as SelectList;
            Assert.That(products, Is.Not.Null);
            Assert.That(products!.Select(i => i.Text),
                Is.EquivalentTo(new[] { "Laptop", "Smartphone", "Novel" }));
            Assert.That(products!.Select(i => i.Value),
                Is.EquivalentTo(new[] { "1", "2", "3" }));
        }

        [Test]
        public void Create_Get_NoProducts_ReturnsEmptyDropdown()
        {
            using var emptyContext = TestDbContextFactory.Create();
            using var controller = new OrderController(emptyContext);

            controller.Create();

            var products = controller.ViewData["Products"] as SelectList;
            Assert.That(products, Is.Not.Null);
            Assert.That(products!, Is.Empty);
        }


        [Test]
        public void Create_Post_ValidModel_AddsOrderAndRedirectsToProductIndex()
        {
            var order = new Order
            {
                OrderDate = new DateTime(2024, 11, 1),
                Items = new List<OrderItem>
                {
                    new OrderItem { ProductId = 1, Quantity = 2 },
                    new OrderItem { ProductId = 3, Quantity = 1 }
                }
            };

            var result = _controller.Create(order);

            var redirect = result as RedirectToActionResult;
            Assert.That(redirect, Is.Not.Null);
            Assert.That(redirect!.ActionName, Is.EqualTo("Index"));
            Assert.That(redirect.ControllerName, Is.EqualTo("Product"));

            var saved = _context.Orders.Include(o => o.Items).Single();
            Assert.That(saved.OrderDate, Is.EqualTo(new DateTime(2024, 11, 1)));
            Assert.That(saved.Items, Has.Count.EqualTo(2));
            Assert.That(_context.OrderItems.Count(), Is.EqualTo(2));
        }

        [Test]
        public void Create_Post_InvalidModel_ReturnsViewWithOrderAndDoesNotSave()
        {
            var order = new Order { OrderDate = DateTime.Today };
            _controller.ModelState.AddModelError("OrderDate", "Invalid date");

            var result = _controller.Create(order);

            var view = result as ViewResult;
            Assert.That(view, Is.Not.Null);
            Assert.That(view!.Model, Is.SameAs(order));
            Assert.That(_controller.ViewData["Products"], Is.InstanceOf<SelectList>());
            Assert.That(_context.Orders.Count(), Is.EqualTo(0));
        }

        [Test]
        public void Create_Post_MultipleOrders_AllOrdersAreListedInDatabase()
        {
            _controller.Create(new Order { OrderDate = DateTime.Today });
            _controller.Create(new Order { OrderDate = DateTime.Today.AddDays(-1) });

            var orders = _context.Orders.OrderBy(o => o.OrderDate).ToList();

            Assert.That(orders, Has.Count.EqualTo(2));
            Assert.That(orders[0].OrderDate, Is.EqualTo(DateTime.Today.AddDays(-1)));
        }
    }
}
