using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using shopapp.data.Abstract;
using shopapp.entity;

namespace shopapp.data.Concrete.EfCore
{
    public class EfCoreProductRepository : EfCoreGenericRepository<Product>, IProductRepository
    {
        public EfCoreProductRepository(ShopContext context) : base(context)
        {
            
        }

        private ShopContext ShopContext 
        {
            get { return context as ShopContext; }
        }

        public Product GetByIdWithCategories(int id)
        {
            return ShopContext.Products
                .Where(i => i.ProductId == id)
                .Include(i => i.ProductCategories)
                .ThenInclude(i => i.Category)
                .FirstOrDefault();
            
        }

        public int GetCountByCategory(string category)
        {
            var products = ShopContext.Products
                .Where(i => i.IsApproved)
                .AsQueryable();

            if (!string.IsNullOrEmpty(category))
            {
                products = products
                    .Include(i => i.ProductCategories)
                    .ThenInclude(i => i.Category)
                    .Where(i => i.ProductCategories.Any(a => a.Category.Url == category));
            }
            return products.Count();
        }

        public List<Product> GetHomePageProducts()
        {
            return ShopContext.Products
                .Where(i => i.IsHome && i.IsApproved).ToList();
        }

        public Product GetProductDetails(string url)
        {
            return ShopContext.Products
                .Where(i => i.Url == url)
                .Include(i => i.ProductCategories)
                .ThenInclude(i => i.Category)
                .FirstOrDefault();
        }

        public List<Product> GetProductsByCategory(string name, int page, int pageSize)
        {
            var products = ShopContext.Products
                .Where(i => i.IsApproved)
                .AsQueryable();
            if (!string.IsNullOrEmpty(name))
            {
                products = products
                    .Include(i => i.ProductCategories)
                    .ThenInclude(i => i.Category)
                    .Where(i => i.ProductCategories.Any(a => a.Category.Url == name));
            }
            return products.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        }

        public List<Product> GetSearchResult(string searchString, int page, int pageSize, int min, int max, int catId)
        {
            if (catId != 0)
            {
                var products = ShopContext.Products
                .Where(i => i.IsApproved && (i.Name.ToLower().Contains(searchString.ToLower()) || i.Description.ToLower().Contains(searchString.ToLower())) && (i.Price >= min && i.Price <= max) && i.ProductCategories.Any(a => a.CategoryId == catId))
                .AsQueryable();

                return products.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            } else {
                var products = ShopContext.Products
                .Where(i => i.IsApproved && (i.Name.ToLower().Contains(searchString.ToLower()) || i.Description.ToLower().Contains(searchString.ToLower())) && (i.Price >= min && i.Price <= max))
                .AsQueryable();

                return products.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            }     
        }

        public void Update(Product entity, int[] categoryIds)
        {
            var product = ShopContext.Products
                .Include(i => i.ProductCategories)
                .FirstOrDefault(i => i.ProductId == entity.ProductId);

            if (product != null)
            {
                product.Name = entity.Name;
                product.Price = entity.Price;
                product.Description = entity.Description;
                product.Url = entity.Url;
                product.ImageUrl = entity.ImageUrl;
                product.IsApproved = entity.IsApproved;
                product.IsHome = entity.IsHome;

                product.ProductCategories = categoryIds.Select(cid => new ProductCategory
                {
                    ProductId = entity.ProductId,
                    CategoryId = cid
                }).ToList();
            }
        }

        public void Create(Product entity, int[] categoryIds)
        {
            var product = new Product {
                Name = entity.Name,
                Price = entity.Price,
                Description = entity.Description,
                Url = entity.Url,
                ImageUrl = entity.ImageUrl,
                IsApproved = entity.IsApproved,
                IsHome = entity.IsHome,
                ProductCategories = categoryIds.Select(cid => new ProductCategory
                {
                    ProductId = entity.ProductId,
                    CategoryId = cid
                }).ToList()
            };
            string sqlcmd = "INSERT INTO products (Name,Url,Price,Description,ImageUrl,IsApproved,IsHome) VALUES ({0},{1},{2},{3},{4},{5},{6})";
            ShopContext.Database.ExecuteSqlRaw(sqlcmd,product.Name,product.Url,product.Price,product.Description,product.ImageUrl,product.IsApproved,product.IsHome);

            var lastProductId = ShopContext.Products.OrderByDescending(p => p.ProductId).Select(p => p.ProductId).FirstOrDefault();
            foreach (var item in product.ProductCategories)
            {
                string sqlStr = "INSERT INTO productcategory (CategoryId,ProductId) VALUES ({0},{1})";
                ShopContext.Database.ExecuteSqlRaw(sqlStr,item.CategoryId,lastProductId);
            }      
        }

        public int GetCountBySearch(string searchString, int min, int max, int catId)
        {
            if (catId != 0)
            {
                var products = ShopContext.Products
                .Where(i => i.IsApproved && (i.Name.ToLower().Contains(searchString.ToLower()) || i.Description.ToLower().Contains(searchString.ToLower())) && (i.Price >= min && i.Price <= max) && i.ProductCategories.Any(a => a.CategoryId == catId))
                .AsQueryable();

                return products.Count();
            } else {
                var products = ShopContext.Products
                .Where(i => i.IsApproved && (i.Name.ToLower().Contains(searchString.ToLower()) || i.Description.ToLower().Contains(searchString.ToLower())) && (i.Price >= min && i.Price <= max))
                .AsQueryable();

                return products.Count();
            }
            
        }

        public List<string> Chart1Labels(string catId = "0")
        {
            if (catId == "0")
            {
                var result = ShopContext.Products
                    .Join(ShopContext.OrderItems, p => p.ProductId, oi => oi.ProductId, (p, oi) => new { p.Name, oi.Quantity })
                    .GroupBy(x => x.Name)
                    .OrderByDescending(g => g.Sum(x => x.Quantity))
                    .Take(7)
                    .Select(g => g.Key)
                    .ToList();

                return result;
            }
            else
            {
                int categoryId = int.Parse(catId);

                var result = ShopContext.Products
                    .Join(ShopContext.OrderItems, p => p.ProductId, oi => oi.ProductId, (p, oi) => new { p, oi.Quantity })
                    .Join(ShopContext.ProductCategory, x => x.p.ProductId, pc => pc.ProductId, (x, pc) => new { x.p.Name, x.Quantity, pc.CategoryId })
                    .Where(x => x.CategoryId == categoryId)
                    .GroupBy(x => x.Name)
                    .OrderByDescending(g => g.Sum(x => x.Quantity))
                    .Take(7)
                    .Select(g => g.Key)
                    .ToList();

                return result;
            }
        }

        public List<int> Chart1Datas(string catId = "0")
        {
            if (catId == "0")
            {
                var result = ShopContext.Products
                    .Join(ShopContext.OrderItems, p => p.ProductId, oi => oi.ProductId, (p, oi) => new { p.Name, oi.Quantity })
                    .GroupBy(x => x.Name)
                    .OrderByDescending(g => g.Sum(x => x.Quantity))
                    .Take(7)
                    .Select(g => g.Sum(x => x.Quantity))
                    .ToList();

                return result;
            }
            else
            {
                int categoryId = int.Parse(catId);

                var result = ShopContext.Products
                    .Join(ShopContext.OrderItems, p => p.ProductId, oi => oi.ProductId, (p, oi) => new { p, oi.Quantity })
                    .Join(ShopContext.ProductCategory, x => x.p.ProductId, pc => pc.ProductId, (x, pc) => new { x.p.Name, x.Quantity, pc.CategoryId })
                    .Where(x => x.CategoryId == categoryId)
                    .GroupBy(x => x.Name)
                    .OrderByDescending(g => g.Sum(x => x.Quantity))
                    .Take(7)
                    .Select(g => g.Sum(x => x.Quantity))
                    .ToList();

                return result;
            }
        }

        public List<string> Chart2Labels(string date1, string date2)
        {
            List<string> result = new List<string>();

            if (date1 == "0" || date2 == "0")
            {
                var query = ShopContext.Orders
                    .Join(ShopContext.OrderItems,
                        o => o.Id,
                        oi => oi.OrderId,
                        (o, oi) => new { o.OrderDate, oi.Price, oi.Quantity })
                    .GroupBy(x => x.OrderDate)
                    .Select(g => new
                    {
                        OrderDate = g.Key,
                        Total = g.Sum(x => x.Price * x.Quantity)
                    })
                    .OrderByDescending(g => g.OrderDate)
                    .Take(7);

                result = query
                    .Select(x => x.OrderDate.ToString("dd-MM-yyyy"))
                    .ToList();
            }
            else
            {
                DateTime startDate = DateTime.Parse(date1);
                DateTime endDate = DateTime.Parse(date2);

                var query = ShopContext.Orders
                    .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
                    .Join(ShopContext.OrderItems,
                        o => o.Id,
                        oi => oi.OrderId,
                        (o, oi) => new { o.OrderDate, oi.Price, oi.Quantity })
                    .GroupBy(x => x.OrderDate)
                    .Select(g => new
                    {
                        OrderDate = g.Key,
                        Total = g.Sum(x => x.Price * x.Quantity)
                    })
                    .OrderBy(g => g.OrderDate);

                result = query
                    .Select(x => x.OrderDate.ToString("dd-MM-yyyy"))
                    .ToList();
            }

            return result;
        }

        public List<int> Chart2DataTotal(string date1, string date2)
        {
            List<int> result = new List<int>();

            if (date1 == "0" || date2 == "0")
            {
                var query = ShopContext.Orders
                    .Join(ShopContext.OrderItems,
                        o => o.Id,
                        oi => oi.OrderId,
                        (o, oi) => new { o.OrderDate, oi.Price, oi.Quantity })
                    .GroupBy(x => x.OrderDate)
                    .Select(g => new
                    {
                        OrderDate = g.Key,
                        Total = g.Sum(x => x.Price * x.Quantity)
                    })
                    .OrderByDescending(g => g.OrderDate)
                    .Take(7);

                result = query
                    .Select(x => (int)x.Total)
                    .ToList();
            }
            else
            {
                DateTime startDate = DateTime.Parse(date1);
                DateTime endDate = DateTime.Parse(date2);

                var query = ShopContext.Orders
                    .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
                    .Join(ShopContext.OrderItems,
                        o => o.Id,
                        oi => oi.OrderId,
                        (o, oi) => new { o.OrderDate, oi.Price, oi.Quantity })
                    .GroupBy(x => x.OrderDate)
                    .Select(g => new
                    {
                        OrderDate = g.Key,
                        Total = g.Sum(x => x.Price * x.Quantity)
                    })
                    .OrderBy(g => g.OrderDate);

                result = query
                    .Select(x => (int)x.Total)
                    .ToList();
            }

            return result;
        }

        public List<string> Chart3Labels()
        {
            var result = (from o in ShopContext.Orders
                join oi in ShopContext.OrderItems on o.Id equals oi.OrderId
                group new { o, oi } by new { o.Email, FullName = o.FirstName + " " + o.LastName } into g
                orderby g.Count() descending
                select g.Key.FullName).Take(5).ToList();

            return result;
        }

        public List<int> Chart3Datas()
        {
            var result = (from o in ShopContext.Orders
                join oi in ShopContext.OrderItems on o.Id equals oi.OrderId
                group oi by new { o.Email, FullName = o.FirstName + " " + o.LastName } into g
                orderby g.Count() descending
                select g.Count()).Take(5).ToList();

            return result;
        }

        public List<string> Chart4Labels()
        {
            var result = (from p in ShopContext.Products
                join pc in ShopContext.ProductCategory on p.ProductId equals pc.ProductId
                join c in ShopContext.Categories on pc.CategoryId equals c.CategoryId
                join oi in ShopContext.OrderItems on p.ProductId equals oi.ProductId
                group oi by c.Name into g
                select g.Key).ToList();

            return result;
        }

        public List<int> Chart4Datas()
        {
            var totalQuantity = ShopContext.OrderItems.Sum(oi => oi.Quantity);
            var result = (from p in ShopContext.Products
                join pc in ShopContext.ProductCategory on p.ProductId equals pc.ProductId
                join c in ShopContext.Categories on pc.CategoryId equals c.CategoryId
                join oi in ShopContext.OrderItems on p.ProductId equals oi.ProductId
                group oi by c.Name into g
                select Math.Round((g.Sum(oi => oi.Quantity) / (decimal)totalQuantity) * 100, 2)).Select(Convert.ToInt32).ToList();

            return result;
        }
    }
}