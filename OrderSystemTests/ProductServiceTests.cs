using NUnit.Framework;
using OrderSystem.Data;
using OrderSystem.Services;

namespace OrderSystemTests
{
    [TestFixture]
    public class ProductServiceTests
    {
        private ProductDbContext _context = null!;
        private ProductService _service = null!;

        [SetUp]
        public void SetUp()
        {
            _context = TestDbContextFactory.CreateSeeded();
            _service = new ProductService(_context);
        }

        [TearDown]
        public void TearDown() => _context.Dispose();

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void SearchProductsByName_EmptySubstring_ReturnsAllProducts(string? substring)
        {
            var result = _service.SearchProductsByName(substring!).ToList();

            Assert.That(result, Has.Count.EqualTo(3));
        }

        [Test]
        public void SearchProductsByName_MatchingSubstring_IsCaseInsensitive()
        {
            var result = _service.SearchProductsByName("LAP").ToList();

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Name, Is.EqualTo("Laptop"));
        }

        [Test]
        public void SearchProductsByName_IncludesCategory()
        {
            var result = _service.SearchProductsByName("Novel").Single();

            Assert.That(result.Category, Is.Not.Null);
            Assert.That(result.Category!.Name, Is.EqualTo("Books"));
        }

        [Test]
        public void SearchProductsByName_NoMatch_ReturnsEmpty()
        {
            var result = _service.SearchProductsByName("Tablet");

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetProductsByCategory_ExistingCategory_ReturnsOnlyItsProducts()
        {
            var result = _service.GetProductsByCategory(1).ToList();

            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result, Has.All.Matches<OrderSystem.Models.Product>(p => p.CategoryId == 1));
        }

        [Test]
        public void GetProductsByCategory_CategoryWithoutProducts_ReturnsEmpty()
        {
            var result = _service.GetProductsByCategory(3);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetProductsSorted_ByName_ReturnsAlphabeticalOrder()
        {
            var names = _service.GetProductsSorted("Name").Select(p => p.Name);

            Assert.That(names, Is.EqualTo(new[] { "Laptop", "Novel", "Smartphone" }));
        }

        [Test]
        public void GetProductsSorted_ByPrice_ReturnsAscendingPrices()
        {
            var prices = _service.GetProductsSorted("Price").Select(p => p.Price).ToList();

            Assert.That(prices, Is.Ordered.Ascending);
            Assert.That(prices.First(), Is.EqualTo(14.99M));
        }

        [Test]
        public void SearchProducts_MatchingTerm_ReturnsMatches()
        {
            var result = _service.SearchProducts("phone").ToList();

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Name, Is.EqualTo("Smartphone"));
        }

        [Test]
        public void SearchProducts_NoMatch_ReturnsEmpty()
        {
            var result = _service.SearchProducts("Tablet");

            Assert.That(result, Is.Empty);
        }
    }
}
