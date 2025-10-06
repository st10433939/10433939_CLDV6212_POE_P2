using _10433939_CLDV6212_POE_P2.Models;
using _10433939_CLDV6212_POE_P2.Services;
using Microsoft.AspNetCore.Mvc;

namespace _10433939_CLDV6212_POE_P2.Controllers
{
    public class ProductController : Controller
    {
        private readonly BlobService _blobService;
        private readonly TableStorageService _tableStorageService;

        public ProductController(BlobService blobService, TableStorageService tableStorageService)
        {
            _blobService = blobService;
            _tableStorageService = tableStorageService;
        }
        public async Task<IActionResult> Index()
        {
            var products = await _tableStorageService.GetAllProductsAsync();
            return View(products);
        }
        //Add
        [HttpPost]
        public async Task<IActionResult> AddProduct(Product product, IFormFile file)
        {
            if (file != null)
            {
                using var stream = file.OpenReadStream();
                var imageUrl = await _blobService.UploadsAsync(stream, file.FileName);
                product.ImageUrl = imageUrl;
            }

            if (ModelState.IsValid)
            {
                product.PartitionKey = "ProductPartition";
                product.RowKey = Guid.NewGuid().ToString();

                await _tableStorageService.addProductAsync(product);
                return RedirectToAction("Index");
            }
            return View(product);
        }

        //Delete
        [HttpPost]
        public async Task<IActionResult> DeleteProduct(string partitionKey, string rowKey, Product product)
        {
            if (product != null && !string.IsNullOrEmpty(product.ImageUrl))
            {
                //Delete blob image
                await _blobService.DeleteBlobAsync(product.ImageUrl);
            }
            //Delete from table
            await _tableStorageService.DeleteProductAsync(partitionKey, rowKey);
            return RedirectToAction("Index");
        }
        [HttpGet]
        public IActionResult AddProduct()
        {
            return View();
        }
    }
}
