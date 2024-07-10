using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using shopapp.business.Abstract;
using shopapp.entity;
using shopapp.webui.Identity;
using shopapp.webui.Models;

namespace shopapp.webui.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private IProductService _productService;
        private ICategoryService _categoryService;
        private RoleManager<IdentityRole> _roleManager;
        private UserManager<User> _userManager;
        public AdminController(IProductService productService, 
            ICategoryService categoryService,
            RoleManager<IdentityRole> roleManager,
            UserManager<User> userManager
        )
        {
            _productService = productService;
            _categoryService = categoryService;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task<IActionResult> UserDelete(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user!=null)
            {
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    CreateMessage($"{user.FirstName} {user.LastName} kullancısı silindi.","danger");
                    return RedirectToAction("UserList","Admin");
                } else {
                    foreach (var err in result.Errors)
                    {
                        CreateMessage($"Bir hata oluştu {err}","danger");     
                    }
                    return RedirectToAction("UserList","Admin");
                }
            }
            return View();
        }
        
        public async Task<IActionResult> UserEdit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user!=null)
            {
                var selectedroles = await _userManager.GetRolesAsync(user);
                var roles = _roleManager.Roles.Select(i => i.Name);
                ViewBag.Roles = roles;
                return View(new UserDetailsModel(){
                    UserId = user.Id,
                    UserName = user.UserName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    EmailConfirmed = user.EmailConfirmed,
                    SelectedRoles = selectedroles
                });
            }
            return Redirect("~/admin/user/list");
        }
        [HttpPost]
        public async Task<IActionResult> UserEdit(UserDetailsModel model, string[] selectedroles)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.UserId);
                if (user!=null)
                {
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.UserName = model.UserName;
                    user.Email = model.Email;
                    user.EmailConfirmed = model.EmailConfirmed;

                    var result = await _userManager.UpdateAsync(user);

                    if (result.Succeeded)
                    {
                        var userRoles = await _userManager.GetRolesAsync(user);
                        selectedroles = selectedroles?? new string[]{};
                        await _userManager.AddToRolesAsync(user,selectedroles.Except(userRoles).ToArray<string>());
                        await _userManager.RemoveFromRolesAsync(user,userRoles.Except(selectedroles).ToArray<string>());
                        CreateMessage($"{model.UserName} kullanıcısı başarıyla güncellendi.","info");
                        return Redirect("/admin/user/list");
                    }
                }
                CreateMessage($"Bir hata oluşttu","danger");
                return View(model);
            }
            CreateMessage($"Bir hata oluşttu","danger");
            return View(model);
        }
        public IActionResult UserList()
        {
            return View(_userManager.Users);
        }
        public async Task<IActionResult> RoleDelete(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role != null)
            {
                var result =  await _roleManager.DeleteAsync(role);
                if (result.Succeeded)
                {
                    CreateMessage($"{role.Name} isimli rol silindi.","danger");
                    return RedirectToAction("RoleList","Admin");
                }
                foreach (var err in result.Errors)
                {
                    CreateMessage($"{err.Description}","danger");
                }
            }
            CreateMessage($"Bir hata oluştu","danger");
            return RedirectToAction("RoleList","Admin");
        }

        public async Task<IActionResult> RoleEdit(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            var members = new List<User>();
            var nonmembers = new List<User>();
            
            foreach (var user in _userManager.Users)
            {
                var list = await _userManager.IsInRoleAsync(user,role.Name) ? members : nonmembers;
                list.Add(user);
            }
            var model = new RoleDetails()
            {
                Role = role,
                Members = members,
                NonMembers = nonmembers
            };
            return View(model);

        }
        
        [HttpPost]
        public async Task<IActionResult> RoleEdit(RoleEditModel model)
        {
            if (ModelState.IsValid)
            {
                foreach (var userId in model.IdsToAdd ?? new string[]{})
                {
                    var user = await _userManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        var result = await _userManager.AddToRoleAsync(user,model.RoleName);
                        if (!result.Succeeded)
                        {
                            foreach (var err in result.Errors)
                            {
                                CreateMessage($"Ekleme işlemi başarısız. {err}","danger");
                            }
                        }
                    }
                }

                foreach (var userId in model.IdsToRemove ?? new string[]{})
                {
                    var user = await _userManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        var result = await _userManager.RemoveFromRoleAsync(user,model.RoleName);
                        if (!result.Succeeded)
                        {
                            foreach (var err in result.Errors)
                            {
                                CreateMessage($"Kaldırma işlemi başarısız. {err}","danger");
                            }
                        }
                    }
                }
            }
            return Redirect("/admin/role/"+model.RoleId);
        }
        public IActionResult RoleList()
        {
            return View(_roleManager.Roles);
        }
        public IActionResult RoleCreate()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> RoleCreate(RoleModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _roleManager.CreateAsync(new IdentityRole(model.Name));
                if (result.Succeeded)
                {
                    CreateMessage($"{model.Name} rolü başarıyla eklendi.","success");
                    return RedirectToAction("RoleList","Admin");
                }
                else
                {
                    foreach (var err in result.Errors)
                    {
                        CreateMessage($"Bir hata oluştu\n{err.Description}","danger");
                    }
                }
            }
            return View(model);
        }

        public async Task<IActionResult> ProductList()
        {
            var products = await _productService.GetAll();
            return View(new ProductListViewModel()
            {
                Products = products
            });
        }
        public async Task<IActionResult> CategoryList()
        {
            var categories = await _categoryService.GetAll();
            return View(new CategoryListViewModel()
            {
                Categories = categories
            });
        }
        [HttpGet]
        public async Task<IActionResult> ProductCreate()
        {
            ViewBag.Categories = await _categoryService.GetAll();
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ProductCreate(ProductModel model, int[] categoryIds, IFormFile file)
        {
            if (ModelState.IsValid)
            {
                var entity = new Product
                {
                    Name = model.Name,
                    Url = model.Url,
                    Description = model.Description,
                    Price = model.Price,
                    ImageUrl = model.ImageUrl,
                    IsApproved = model.IsApproved,
                    IsHome = model.IsHome
                };

                if (file == null)
                {
                    CreateMessage("Resim seçmelisiniz","danger");
                }
                else {
                    var extension = Path.GetExtension(file.FileName);
                    var randomName = string.Format($"{Guid.NewGuid()}{extension}");
                    entity.ImageUrl = randomName;
                    var path = Path.Combine(Directory.GetCurrentDirectory(),"wwwroot\\img",randomName);
                    using(var stream = new FileStream(path,FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    if (_productService.Create(entity, categoryIds))
                    {
                        CreateMessage($"{entity.Name} isimli ürün eklendi.","success");
                        return RedirectToAction("ProductList");
                    }
                    CreateMessage(_productService.ErrorMessage,"danger");
                }     
            }
            
            ViewBag.Categories = await _categoryService.GetAll();
            return View(model);   

        }
        [HttpGet]
        public IActionResult CategoryCreate()
        {
            return View();
        }
        [HttpPost]
        public IActionResult CategoryCreate(CategoryModel model)
        {
            if (ModelState.IsValid)
            {
                var entity = new Category
                {
                    Name = model.Name,
                    Url = model.Url
                };

                _categoryService.Create(entity);
                CreateMessage($"{entity.Name} isimli kategori eklendi.","success");

                return RedirectToAction("CategoryList");
            }
            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> ProductEdit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var entity = _productService.GetByIdWithCategories((int)id);

            if (entity == null)
            {
                return NotFound();
            }

            var model = new ProductModel
            {
                ProductId = entity.ProductId,
                Name = entity.Name,
                Url = entity.Url,
                Price = entity.Price,
                Description = entity.Description,
                IsApproved = entity.IsApproved,
                IsHome = entity.IsHome,
                ImageUrl = entity.ImageUrl,
                SelectedCategories = entity.ProductCategories.Select(i => i.Category).ToList()
            };

            ViewBag.Categories = await _categoryService.GetAll();

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> ProductEdit(ProductModel model, int[] categoryIds, IFormFile file)
        {
            if (ModelState.IsValid)
            {
                var entity = await _productService.GetById(model.ProductId);

                if (entity == null)
                {
                    return NotFound();
                }

                entity.Name = model.Name;
                entity.Url = model.Url;
                entity.Description = model.Description;
                entity.Price = model.Price;
                entity.IsApproved = model.IsApproved;
                entity.IsHome = model.IsHome;

                if (file != null)
                {
                    var extension = Path.GetExtension(file.FileName);
                    var randomName = string.Format($"{Guid.NewGuid()}{extension}");
                    entity.ImageUrl = randomName;
                    var path = Path.Combine(Directory.GetCurrentDirectory(),"wwwroot\\img",randomName);

                    using(var stream = new FileStream(path,FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                }

                if (_productService.Update(entity, categoryIds))
                {
                    CreateMessage($"{entity.Name} isimli kayıt güncellendi.","info");
                    return RedirectToAction("ProductList");
                }
                CreateMessage(_productService.ErrorMessage,"danger");
            }
            ViewBag.Categories = await _categoryService.GetAll();
            return View(model);

        }
        [HttpGet]
        public IActionResult CategoryEdit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var entity = _categoryService.GetByIdWithProducts((int)id);

            if (entity == null)
            {
                return NotFound();
            }

            var model = new CategoryModel
            {
                CategoryId = entity.CategoryId,
                Name = entity.Name,
                Url = entity.Url,
                Products = entity.ProductCategories.Select(p => p.Product).ToList()
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> CategoryEdit(CategoryModel model)
        {
            if (ModelState.IsValid)
            {
                var entity = await _categoryService.GetById(model.CategoryId);

                if (entity == null)
                {
                    return NotFound();
                }

                entity.Name = model.Name;
                entity.Url = model.Url;

                _categoryService.Update(entity);
                CreateMessage($"{entity.Name} isimli kategori güncellendi.","info");

                return RedirectToAction("CategoryList");
            }
            return View(model);

        }
        public async Task<IActionResult> DeleteProduct(int productId)
        {
            var entity = await _productService.GetById(productId);
            if (entity != null)
            {
                _productService.Delete(entity);
            }
            CreateMessage($"{entity.Name} isimli ürün silindi.","danger");
            return RedirectToAction("ProductList");
        }
        public async Task<IActionResult> DeleteCategory(int categoryId)
        {
            var entity = await _categoryService.GetById(categoryId);
            if (entity != null)
            {
                _categoryService.Delete(entity);
            }
            CreateMessage($"{entity.Name} kategorisi kaldırıldı.","danger");
            return RedirectToAction("CategoryList");
        }
        [HttpPost]
        public IActionResult DeleteFromCategory(int productId, int categoryId)
        {
            _categoryService.DeleteFromCategory(productId, categoryId);
            return Redirect("/admin/categories/" + categoryId);

        }

        public async Task<IActionResult> Graphics()
        {
            return View(new List<GraphicModel>
            {
               new GraphicModel {
                Datas = _productService.Chart1Datas("0"),
                Labels = _productService.Chart1Labels("0"),
                Category = await _categoryService.GetAll()
               },
               new GraphicModel {
                Datas = _productService.Chart2DataTotal("0","0"),
                Labels = _productService.Chart2Labels("0","0")
               },
               new GraphicModel {
                Datas = _productService.Chart3Datas(),
                Labels = _productService.Chart3Labels() 
               },
               new GraphicModel {
                Datas = _productService.Chart4Datas(),
                Labels = _productService.Chart4Labels() 
               }
            });
        }

        [HttpPost]
        public async Task<IActionResult> Graphics(string catId = "0", string date1 = "0", string date2 = "0")
        {
            return View(new List<GraphicModel>
            {
               new GraphicModel {
                Datas = _productService.Chart1Datas(catId),
                Labels = _productService.Chart1Labels(catId),
                Category = await _categoryService.GetAll()
               },
               new GraphicModel {
                Datas = _productService.Chart2DataTotal(date1,date2),
                Labels = _productService.Chart2Labels(date1,date2)
               },
               new GraphicModel {
                Datas = _productService.Chart3Datas(),
                Labels = _productService.Chart3Labels() 
               },
               new GraphicModel {
                Datas = _productService.Chart4Datas(),
                Labels = _productService.Chart4Labels() 
               }
            });
        }

        private void CreateMessage(string message, string alertclass)
        {
            var msg = new AlertMessage
            {
                Message = message,
                ClassType = alertclass
            };

            TempData["message"] = JsonConvert.SerializeObject(msg);
        }
    }
}