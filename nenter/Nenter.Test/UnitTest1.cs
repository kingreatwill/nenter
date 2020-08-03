using System;
using Nenter.Core;
using Nenter.Core.Extensions;
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
        public void TestObjectId()
        {
            // NewId
            for (int i = 0; i < 100; i++)
            {
                var oid = ObjectId.NewId();
                Console.WriteLine(oid);
            }
            // 生产/解包
            {
                var sourceId = ObjectId.NewId();
                var reverseId = new ObjectId(sourceId);
            }
            // 隐式转换
            {
                var sourceId = ObjectId.NewId();

                // 转换为 string
                var stringId = sourceId;
                string userId = ObjectId.NewId();

                // 转换为 ObjectId
                ObjectId id = stringId;
            }
            Assert.Pass();
        }

        [Test]
        public void Test1()
        {
            var o = new TestIExtendableObject();
            o.SetData("name", "1");
            o.SetData("name1", new { });

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

        [Test]
        public void Test3()
        {
            Console.WriteLine(GuidExtensions.NextGuid());
            Console.WriteLine(GuidExtensions.NextGuid());
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
    public class TestAbs2 : TestAbs
    {
        public override string Get()
        {
            return base.Get() + "123";
        }
    }
}