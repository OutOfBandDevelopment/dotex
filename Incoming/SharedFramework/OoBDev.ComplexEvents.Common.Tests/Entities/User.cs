using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace OoBDev.ComplexEvents.Common.Tests.Entities
{
    [Table("Users", Schema = "Core")]
    public class User
    {
        [Key, Required, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid UserId { get; set; }

        [MaxLength(100)]
        public string Username { get; set; }

        public static object[] MasterData => new[]
        {
                new {  UserId= new Guid( "DDE27BBD-5647-4611-B662-088C7FBA3E55"), Username= "SYSTEM" },
                new {  UserId= new Guid( "20000000-0000-0000-0000-000000000002"), Username= "IC Import" },
            };
    }
}
