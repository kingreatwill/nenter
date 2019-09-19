using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlClient;
using System.Linq;
using Nenter.Dapper.Linq.Extensions;
using NUnit.Framework;

namespace Nenter.Dapper.Linq.Test
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
            var _connection = new SqlConnection("Server=(local);Initial Catalog=NenterTest;Integrated Security=True");
            try
            {
                var s = _connection.Query<Addresses>();
                var m = s.Single();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
           
            Assert.Pass();
        }
    }
    [Table("Addresses")]
    public class Addresses
    {
        [Key]
        public int Id { get; set; }
        public string Street { get; set; }
        public string CityId { get; set; }
    }
}