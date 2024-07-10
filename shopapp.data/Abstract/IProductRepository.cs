using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using shopapp.entity;

namespace shopapp.data.Abstract
{
    public interface IProductRepository : IRepository<Product>
    {
        Product GetProductDetails(string url);
        Product GetByIdWithCategories(int id);
        List<Product> GetProductsByCategory(string name, int page, int pageSize);
        List<Product> GetSearchResult(string searchString, int page, int pageSize, int min, int max, int catId);
        List<Product> GetHomePageProducts();
        int GetCountByCategory(string category);
        int GetCountBySearch(string searchString, int min, int max, int catId);
        void Update(Product entity, int[] categoryIds);
        void Create(Product entity, int[] categoryIds);
        List<string> Chart1Labels (string catId);
        List<int> Chart1Datas (string catId);
        List<string> Chart2Labels (string date1, string date2);
        List<int> Chart2DataTotal (string date1, string date2);
        List<string> Chart3Labels ();
        List<int> Chart3Datas ();
        List<string> Chart4Labels ();
        List<int> Chart4Datas ();
    }
}