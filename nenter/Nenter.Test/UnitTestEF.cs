using Nenter.Data.EntityFramework;
using NUnit.Framework;

namespace Nenter.Test
{
    public class UnitTestEF
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            var _dbContext = new MyContext();
            var t = typeof(Blog);
            var df = _dbContext.Model.FindEntityType(nameof(t));
            Assert.Pass();
        }
    }
}