using _10433939_CLDV6212_POE_P2.Data;
using _10433939_CLDV6212_POE_P2.Models;
using _10433939_CLDV6212_POE_P2.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace _10433939_CLDV6212_POE_P2.Controllers
{
    public class CustomerController : Controller
    {
        public readonly TableStorageService _tableStorageService;
        //public readonly ApplicationDbContext _dbContext;

        public CustomerController(TableStorageService tableStorageService)
        {
            _tableStorageService = tableStorageService;
            //_dbContext = applicationDbContext;
        }

        public async Task<IActionResult> Index()
        {
            var customers = await _tableStorageService.GetAllCustomersAsync();
            //View(await _dbContext.Customer.ToListAsync());
            return View(customers);
        }
        //Delete
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            await _tableStorageService.DeleteCustomerAsync(partitionKey, rowKey);
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> AddCustomer(Customer customer)
        {
            customer.PartitionKey = "CustomersPartition";
            customer.RowKey = Guid.NewGuid().ToString();

            await _tableStorageService.AddCustomerAsync(customer);
            return RedirectToAction("Index");
        }
        [HttpGet]
        public IActionResult AddCustomer()
        {
            return View();
        }
    }
}
