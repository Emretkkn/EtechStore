using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using shopapp.entity;

namespace shopapp.business.Abstract
{
    public interface IProductService : IValidator<Product>
    {
        Task<Product> GetById(int id);
        Product GetByIdWithCategories(int id);
        Product GetProductDetails(string url);
        List<Product> GetProductsByCategory(string name, int page, int pageSize);
        Task<List<Product>> GetAll();
        List<Product> GetHomePageProducts();
        List<Product> GetSearchResult(string searchString, int page, int pageSize, int min, int max, int catId);
        bool Create(Product entity);
        bool Create(Product entity, int[] categoryIds);
        Task<Product> CreateAsync(Product entity);
        void Update(Product entity);
        bool Update(Product entity, int[] categoryIds);
        Task UpdateAsync(Product entityToUpdate, Product entity);
        void Delete(Product entity);
        Task DeleteAsync(Product entity);
        int GetCountByCategory(string category);
        int GetCountBySearch(string searchString, int min, int max, int catId);
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