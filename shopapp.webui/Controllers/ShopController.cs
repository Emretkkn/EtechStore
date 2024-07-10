using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using shopapp.business.Abstract;
using shopapp.entity;
using shopapp.webui.Models;

namespace shopapp.webui.Controllers
{
    public class ShopController : Controller
    {
        private IProductService _productService;
        public ShopController(IProductService productService)
        {
            this._productService = productService;
        }
        // localhost/products/telefon?page=1
        public IActionResult List(string category, int page = 1)
        {
            const int pageSize = 6;
            var ProductViewModel = new ProductListViewModel()
            {
                PageInfo = new PageInfo
                {
                    TotalItems = _productService.GetCountByCategory(category),
                    CurrentPage = page,
                    ItemsPerPage = pageSize,
                    CurrentCategory = category
                },
                Products = _productService.GetProductsByCategory(category, page, pageSize)
            };

            return View(ProductViewModel);
        }
        public IActionResult Details(string url)
        {
            if (url == null)
            {
                return NotFound();
            }
            Product product = _productService.GetProductDetails(url);

            if (product == null)
            {
                return NotFound();
            }

            return View(new ProductDetailModel
            {
                Product = product,
                Categories = product.ProductCategories.Select(i => i.Category).ToList()
            });
        }
        public IActionResult Search(int catId, string q = " ", int min = 0, int max = 200000, int page = 1)
        {
            const int pageSize = 6;
            var ProductViewModel = new ProductListViewModel()
            {
                PageInfo = new PageInfo
                {
                    TotalItems = _productService.GetCountBySearch(q,min,max,catId),
                    CurrentPage = page,
                    ItemsPerPage = pageSize,
                    SearchString = q,
                    Min = min.ToString(),
                    Max = max.ToString(),
                    catId = catId.ToString()
                },
                Products = _productService.GetSearchResult(q,page,pageSize,min,max,catId)
            };
            return View(ProductViewModel);
        }
    }
}