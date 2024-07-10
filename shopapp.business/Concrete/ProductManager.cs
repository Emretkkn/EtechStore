using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using shopapp.business.Abstract;
using shopapp.data.Abstract;
using shopapp.data.Concrete.EfCore;
using shopapp.entity;

namespace shopapp.business.Concrete
{
    public class ProductManager : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        public ProductManager(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public bool Create(Product entity)
        {
            if (Validation(entity))
            {
                _unitOfWork.Products.Create(entity);
                _unitOfWork.Save();
                return true;
            }
            return false;
        }

        public async Task<Product> CreateAsync(Product entity)
        {
            await _unitOfWork.Products.CreateAsync(entity);
            await _unitOfWork.SaveAsync();
            return entity;
        }

        public void Delete(Product entity)
        {
            _unitOfWork.Products.Delete(entity);
            _unitOfWork.Save();
        }

        public async Task<List<Product>> GetAll()
        {
            return  await _unitOfWork.Products.GetAll();
        }

        public async Task<Product> GetById(int id)
        {
            return await _unitOfWork.Products.GetById(id);
        }

        public Product GetByIdWithCategories(int id)
        {
            return _unitOfWork.Products.GetByIdWithCategories(id);
        }

        public int GetCountByCategory(string category)
        {
            return _unitOfWork.Products.GetCountByCategory(category);
        }

        public int GetCountBySearch(string searchString, int min, int max, int catId)
        {
            return _unitOfWork.Products.GetCountBySearch(searchString,min,max,catId);
        }

        public List<Product> GetHomePageProducts()
        {
            return _unitOfWork.Products.GetHomePageProducts();
        }

        public Product GetProductDetails(string url)
        {
            return _unitOfWork.Products.GetProductDetails(url);
        }

        public List<Product> GetProductsByCategory(string name, int page, int pageSize)
        {
            return _unitOfWork.Products.GetProductsByCategory(name, page, pageSize);
        }

        public List<Product> GetSearchResult(string searchString, int page, int pageSize, int min, int max, int catId)
        {
            return _unitOfWork.Products.GetSearchResult(searchString,page,pageSize,min,max,catId);
        }

        public void Update(Product entity)
        {
            _unitOfWork.Products.Update(entity);
            _unitOfWork.Save();
        }

        public bool Update(Product entity, int[] categoryIds)
        {
            if (Validation(entity))
            {
                if (categoryIds.Length == 0)
                {
                    ErrorMessage += "Ürün için en az bir kategori seçmelisiniz.";
                    return false;
                }
                _unitOfWork.Products.Update(entity, categoryIds);
                _unitOfWork.Save();
                return true;
            }
            return false;
            
        }

        public async Task UpdateAsync(Product entityToUpdate, Product entity)
        {
            entityToUpdate.Name = entity.Name;
            entityToUpdate.Price = entity.Price;
            entityToUpdate.Description = entity.Description;
            entityToUpdate.ImageUrl = entity.ImageUrl;
            await _unitOfWork.SaveAsync();
        }

        public string ErrorMessage { get; set; }

        public bool Validation(Product entity)
        {
            var IsValid = true;

            if (string.IsNullOrEmpty(entity.Name))
            {
                ErrorMessage += "Ürün ismi boş bırakılamaz.\n";
                IsValid = false;
            }

            if (entity.Price < 0)
            {
                ErrorMessage += "Ürün fiyatı negatif olamaz.\n";
                IsValid = false;
            }

            return IsValid;
        }

        public bool Create(Product entity, int[] categoryIds)
        {
            if (Validation(entity))
            {
                if (categoryIds.Length == 0)
                {
                    ErrorMessage += "Ürün için en az bir kategori seçmelisiniz.";
                    return false;
                }
                _unitOfWork.Products.Create(entity, categoryIds);
                _unitOfWork.Save();
                return true;
            }
            return false;
        }

        public List<string> Chart1Labels(string catId)
        {
            return _unitOfWork.Products.Chart1Labels(catId);
        }

        public List<int> Chart1Datas(string catId)
        {
            return _unitOfWork.Products.Chart1Datas(catId);
        }

        public List<string> Chart2Labels(string date1, string date2)
        {
            return _unitOfWork.Products.Chart2Labels(date1,date2);
        }

        public List<int> Chart2DataTotal(string date1, string date2)
        {
            return _unitOfWork.Products.Chart2DataTotal(date1,date2);
        }

        public List<string> Chart3Labels()
        {
            return _unitOfWork.Products.Chart3Labels();
        }

        public List<int> Chart3Datas()
        {
            return _unitOfWork.Products.Chart3Datas();
        }

        public List<string> Chart4Labels()
        {
            return _unitOfWork.Products.Chart4Labels();
        }

        public List<int> Chart4Datas()
        {
            return _unitOfWork.Products.Chart4Datas();
        }

        public async Task DeleteAsync(Product entity)
        {
            _unitOfWork.Products.Delete(entity);
            await _unitOfWork.SaveAsync();
        }
    }
}