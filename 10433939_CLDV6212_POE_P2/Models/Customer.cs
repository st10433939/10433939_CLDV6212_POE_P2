using System.ComponentModel.DataAnnotations;
using Azure.Data.Tables;
using System.Diagnostics.Tracing;
using Azure;

namespace _10433939_CLDV6212_POE_P2.Models
{
    public class Customer : ITableEntity
    {

        [Key]
        public int Customer_Id { get; set; }
        public string? Customer_Name { get; set; }
        public string? email { get; set; }
        public string? password { get; set; }

        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
    }
}

