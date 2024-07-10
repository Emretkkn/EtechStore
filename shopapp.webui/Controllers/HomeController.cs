using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using shopapp.data.Abstract;
using shopapp.business.Abstract;
using shopapp.webui.Models;
using System.Net.Http;
using Newtonsoft.Json;
using shopapp.entity;

namespace shopapp.webui.Controllers;

public class HomeController : Controller
{
    private IProductService _productService;
    public HomeController(IProductService productService)
    {
        this._productService = productService;
    }
    public IActionResult Index()
    {
        var ProductViewModel = new ProductListViewModel()
        {
            Products = _productService.GetHomePageProducts()
        };

        return View(ProductViewModel);
    }

    public async Task<IActionResult> GetProductsFromRestApi()
    {
        var products = new List<Product>();
        using (var httpClient = new HttpClient())
        {
            using (var response = await httpClient.GetAsync("http://localhost:5042/api/products"))
            {
                string apiResponse = await response.Content.ReadAsStringAsync();  
                products = JsonConvert.DeserializeObject<List<Product>>(apiResponse);
            } 
        }
    
        return View(products);
    }

    public IActionResult About()
    {
        return View();
    }

    public IActionResult Contact()
    {
        return View("MyView");
    }
}

