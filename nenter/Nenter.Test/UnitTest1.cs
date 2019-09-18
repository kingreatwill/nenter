using Nenter.Data.Entities;
using NUnit.Framework;

namespace Nenter.Test
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            var o = new TestIExtendableObject();
            o.SetData("name", "1");
            o.SetData("name1", new {});

            Assert.AreEqual(o.GetData<int>("name"), 1);
            Assert.NotNull(o.GetData<dynamic>("name1"));
            Assert.Pass();
        }
        
        [Test]
        public void Test2()
        {
            Assert.AreEqual(new TestAbs2().Get(), "321123");
            Assert.Pass();
        }
    }

    public class TestIExtendableObject : IExtendableObject
    {
        public string? ExtensionData { get; set; }
    }
    
    public abstract class TestAbs 
    {
        public virtual string Get()
        {
            return "321";
        }
    }
    public class TestAbs2: TestAbs
    {
        public override string Get()
        {
            return base.Get()+"123";
        }
    }
}