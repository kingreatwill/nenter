using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Nenter.Core.Extensions;
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
        public async Task Test1()
        {
            var _connection = new SqlConnection("Server=(local);Initial Catalog=NenterTest;Integrated Security=True");
            try
            {
                var s = await _connection.Query<Addresses>().CountAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
           
            Assert.Pass();
        }
        
        [Test]
        public void Test2()
        {
            var _connection = new MySqlConnection("Server=192.168.1.50;Port=3306;Database=DemoCloud;Uid=DemoCloudUser;Pwd=123456@lcb;");
            try
            {
                var s = _connection.Query<Supplier>().OrderBy(t=>t.SupplierId).Skip(2).Take(10).Single();
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
    
    public class Supplier
    {
        [Key]
        public long SupplierId { get; set; }
        public string ShortName { get; set; }
        public string Contact { get; set; }
    }
}