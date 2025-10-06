using Azure;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueFunctionBasics
{
    internal class OrderEntity : ITableEntity
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? PicUrl { get; set; }


        public string PartitionKey { get; set; } = "Orders";
        public string RowKey { get; set; } = string.Empty;
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
