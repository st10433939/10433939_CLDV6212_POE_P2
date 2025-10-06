using _10433939_CLDV6212_POE_P2.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;

namespace _10433939_CLDV6212_POE_P2.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public HomeController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            var httpClient = _httpClientFactory.CreateClient();
            var apiBaseUrl = _configuration["FunctionApi:BaseUrl"];
            try
            {
                var httpResponseMessage = await httpClient.GetAsync($"{apiBaseUrl}product");
                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    var product = await JsonSerializer.DeserializeAsync<IEnumerable<Product>>(contentStream, options);
                    return View(product);
                }
            }
            catch (HttpRequestException)
            {
                ViewBag.ErrorMessage = "Could not connect to the API. Please ensure the Azure Function is running.";
                return View(new List<Product>());
            }
            ViewBag.ErrorMessage = "An error occured while retrieving data from the API.";
            return View(new List<Product>());
        }
        [HttpGet]
        public IActionResult AddWithImage()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddWithImage(AddProductWithImage model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var httpClient = _httpClientFactory.CreateClient();
            var apiBaseUrl = _configuration["FunctionApi:BaseUrl"];

            using var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(model.Name), "Name");
            formData.Add(new StringContent(model.Email), "Email");
            if (model.Image != null)
            {
                formData.Add(new StreamContent(model.Image.OpenReadStream()), "Image", model.Image.FileName);
            }
            var httpResponseMessage = await httpClient.PostAsync($"{apiBaseUrl}product-with-image", formData);

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = $"Successfully added {model.Name} with image.";
                return RedirectToAction("Index");
            }
            ModelState.AddModelError(string.Empty, "An error occured while claaing the API.");
            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
