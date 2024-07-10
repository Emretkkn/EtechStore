using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using shopapp.business.Abstract;
using shopapp.webui.EmailServices;
using shopapp.webui.Identity;
using shopapp.webui.Models;

namespace shopapp.webui.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class AccountController : Controller
    {
        private UserManager<User> _userManager;
        private SignInManager<User> _signInManager;
        private IEmailSender _emailSender;
        private ICartService _cartService;
        public AccountController(ICartService cartService, UserManager<User> userManager, SignInManager<User> signInManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _cartService = cartService;
        }
        public IActionResult Login(string returnUrl=null)
        {
            return View(new LoginModel()
            {
                ReturnUrl = returnUrl
            });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // ModelState.AddModelError("","Bu kullanıcı adına sahip bir kullanıcı bulunmamaktadır.");
                ModelState.AddModelError("","Bu e-posta adresine kayıtlı bir kullanıcı bulunmamaktadır.");
                return View(model);
            }
            
            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                ModelState.AddModelError("","Lütfen e-posta adresinize gönderilen linkten hesabınızı onaylayınız.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(user,model.Password,true,false);
            if (result.Succeeded)
            {
                CreateMessage($"Hoşgeldin {user.FirstName}","success");
                return Redirect(model.ReturnUrl??"~/");
            }
            ModelState.AddModelError("","Kullanıcı adı veya parola hatalı.");
            return View(model);
        }
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new User()
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                UserName = model.UserName,
                Email = model.Email  
            };
            var result = await _userManager.CreateAsync(user,model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user,"Customer");
                // generate token
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var url = Url.Action("ConfirmEmail","Account",new {
                    userId = user.Id,
                    token = code
                });
                //email
                await _emailSender.SendEmailAsync(model.Email,"E-Tech Store Hesabınızı Onaylayınız.",$"Lütfen e-posta hesabınızı doğrulamak için linke <a href='http://localhost:5083{url}'>tıklayınız.</a>");
                // Create Cart Object
                _cartService.InitializeCart(user.Id);
                CreateMessage($"{user.FirstName} {user.LastName} kayıt işleminiz başarılı.","success");
                return RedirectToAction("Login","Account");
            } 
            ModelState.AddModelError("","Bilinmeyen bir hata oluştu lütfen tekrar deneyiniz.");
            return View(model);
        }
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index","Home");
        }
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
            {
                CreateMessage("Geçersiz token","danger");
                return View();
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var result = await _userManager.ConfirmEmailAsync(user,token);
                if (result.Succeeded)
                {
                    CreateMessage("Hesabınız başarılı bir şekilde onaylandı.","success");
                    return View();
                }
            }
            CreateMessage("Kayıtlı kullanıcı bulunamadı.","danger");
            return View();
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if(string.IsNullOrEmpty(email))
            {
                CreateMessage("E-posta alanı boş bırakılamaz.","danger");
                return View();
            }
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                CreateMessage("E-posta adresine kayıtlı bir kullanıcı bulunmamaktadır.","danger");
                return View();
            }
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            var url = Url.Action("ResetPassword","Account",new {
                userId = user.Id,
                token = code
            });
            //email
            await _emailSender.SendEmailAsync(email,"Parola Sıfırlama",$"Parolanızı sıfırlamak için linke <a href='http://localhost:5083{url}'>tıklayınız.</a>");
            CreateMessage("E-posta adresinize parola sıfırlama bağlantısı gönderilmiştir.","success");
            return View();
        }

        public IActionResult ResetPassword(string userId, string token)
        {
            if (userId == null || token == null)
            {
                return RedirectToAction("Index","Home");
            }
            
            var model = new ResetPasswordModel {Token=token};
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user==null)
            {
                CreateMessage("E-posta adresine kayıtlı kullanıcı bulunamadı.","danger");
                return RedirectToAction("Index","Home");
            }
            var result = await _userManager.ResetPasswordAsync(user,model.Token,model.Password);
            if (result.Succeeded)
            {
                CreateMessage("Parolanız başarılı şekilde güncellendi","success");
                return RedirectToAction("Login","Account");
            }
            return View(model);
        }

        public IActionResult AccessDenied()
        {
            CreateMessage("Bu alana erişim yetkiniz bulunmamaktadır.","danger");
            return View();
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