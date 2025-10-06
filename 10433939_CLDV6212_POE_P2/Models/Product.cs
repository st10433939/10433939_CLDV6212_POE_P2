using Azure.Data.Tables;
using Azure;
using System.ComponentModel.DataAnnotations;

namespace _10433939_CLDV6212_POE_P2.Models
{
    public class Product : ITableEntity
    {
        [Key]
        public int Product_Id { get; set; }
        public string? Product_Name { get; set; }
        public string? Description { get; set; }
        public string? Price { get; set; }
        public string? ImageUrl { get; set; }


        public string? PartitionKey { get; set; }
        public string? RowKey { get; set; }
        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
    }
}
