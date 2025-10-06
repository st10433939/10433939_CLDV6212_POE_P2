using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;

namespace _10433939_CLDV6212_POE_P2.Models
{
    public class Order : ITableEntity
    {
        [Key]
        public int Order_Id { get; set; }

        public string? PartitionKey { get; set; }
        public string? RowKey { get; set; }
        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }

        //Validation Sample
        [Required(ErrorMessage = "Please select an Account.")]
        public int Customer_Id { get; set; }
        [Required(ErrorMessage = "Please select a Product.")]
        public int Product_Id { get; set; }
        [Required(ErrorMessage = "Please select the date.")]
        public DateTime Order_Date { get; set; }
        [Required(ErrorMessage = "Please select your Shipping Address.")]
        public string? Shipping_Address { get; set; }
    }
}
