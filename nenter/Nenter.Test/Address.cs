using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Nenter.Data.Attributes;
using Nenter.Data.Attributes.Joins;
using Nenter.Data.Attributes.LogicalDelete;

namespace Nenter.Test
{
    [Table("Addresses")]
    public class Address
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Street { get; set; }
//
//        [LeftJoin("Users", "Id", "AddressId")]
//        public List<User> Users { get; set; }

        public string CityId { get; set; }
        
//        [InnerJoin("Cities", "CityId", "Identifier")]
//        public City City { get; set; }
    }
    
//    [Table("Users")]
//    public class User : BaseEntity<int>
//    {
//        public string ReadOnly => "test";
//
//        [Column(Order = 1)]
//        public string Name { get; set; }
//
//        public int AddressId { get; set; }
//
//        public int PhoneId { get; set; }
//
//        public int OfficePhoneId { get; set; }
//
//        [LeftJoin("Cars", "Id", "UserId")]
//        public List<Car> Cars { get; set; }
//
//        [LeftJoin("Addresses", "AddressId", "Id")]
//        public Address Addresses { get; set; }
//
//        [InnerJoin("Phones", "PhoneId", "Id", "DAB")]
//        public Phone Phone { get; set; }
//
//        [InnerJoin("Phones", "OfficePhoneId", "Id", "DAB")]
//        public Phone OfficePhone { get; set; }
//
//        [Status]
//        [Deleted]
//        public bool Deleted { get; set; }
//
//        [UpdatedAt]
//        public DateTime? UpdatedAt { get; set; }
//    }
}