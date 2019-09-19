using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using MySql.Data.MySqlClient;
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
                var s = _connection.Query<Addresses>(t=>t.Id==1).OrderBy(t=>t.Id).Single();
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
                var s = _connection.Query<Supplier>(t=>t.SupplierId==1).OrderBy(t=>t.SupplierId).Single();
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