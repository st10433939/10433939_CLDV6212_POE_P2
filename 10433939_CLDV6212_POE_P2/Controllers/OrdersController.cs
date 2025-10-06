using _10433939_CLDV6212_POE_P2.Models;
using _10433939_CLDV6212_POE_P2.Services;
using Microsoft.AspNetCore.Mvc;

namespace _10433939_CLDV6212_POE_P2.Controllers
{
    public class OrdersController : Controller
    {
        private readonly TableStorageService _tableStorageService;
        private readonly QueueService _queueService;

        public OrdersController(TableStorageService tableStorageService, QueueService queueService)
        {
            _tableStorageService = tableStorageService;
            _queueService = queueService;
        }
        public async Task<IActionResult> Index()
        {
            var orders = await _tableStorageService.GetAllOrdersAsync();
            return View(orders);
        }
        public async Task<IActionResult> Receipt()
        {
            var customers = await _tableStorageService.GetAllCustomersAsync();
            var products = await _tableStorageService.GetAllProductsAsync();
            //Check for null or empty list
            if (customers == null || customers.Count == 0)
            {
                //Handle the case where no customers are found
                ModelState.AddModelError("", "No Customers found. Please add customers first.");
                return View();
            }
            if (products == null || products.Count == 0)
            {
                //Handle the case where no products are found
                ModelState.AddModelError("", "No Products found. Please add products first.");
                return View();
            }
            ViewData["Customer"] = customers;
            ViewData["Product"] = products;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Receipt(Order order)
        {
            if (ModelState.IsValid)
            {
                //TableService
                order.Order_Date = DateTime.SpecifyKind(order.Order_Date, DateTimeKind.Utc);
                order.PartitionKey = "OrderPartition";
                order.RowKey = Guid.NewGuid().ToString();
                await _tableStorageService.AddOrderAsync(order);
                //MessageQueue
                string message = $"New order by customers {order.Customer_Id}" +
                    $" of the product {order.Product_Id}" +
                    $" at {order.Shipping_Address} on {order.Order_Date}.";
                await _queueService.SendMessage(message);
                return RedirectToAction("Index");
            }
            //Reload lists if validation fails
            var customers = await _tableStorageService.GetAllCustomersAsync();
            var products = await _tableStorageService.GetAllProductsAsync();
            ViewData["Customer"] = customers;
            ViewData["Product"] = products;
            return View(order);
        }
    }
}
