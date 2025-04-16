using BlogProjectMVC2.Data;
using BlogProjectMVC2.Enum;
using BlogProjectMVC2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace BlogProjectMVC2.Controllers
{
    public class RegisterController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RegisterController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Register
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // POST: Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(Users model)
        {
            if (ModelState.IsValid)
            {
                // Kullanıcı adı kontrolü
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == model.UserName);
                if (existingUser != null)
                {
                    ModelState.AddModelError("UserName", "Bu kullanıcı adı zaten kullanılıyor.");
                    return View(model);
                }

                // Email kontrolü
                existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Bu email adresi zaten kullanılıyor.");
                    return View(model);
                }

                try
                {
                    // Şifreyi doğrudan kaydet
                    model.Password = model.Password;

                    model.RegistrationDate = DateTime.Now;
                    model.IsApproved = true;
                    model.Role = RoleEnum.Yazar;

                    _context.Users.Add(model);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Kayıt işlemi başarılı! Giriş yapabilirsiniz.";
                    return RedirectToAction("Index", "Login");
                }
                catch (Exception)
                {
                    ModelState.AddModelError(string.Empty, "Kayıt sırasında bir hata oluştu. Lütfen tekrar deneyin.");
                }
            }

            return View(model);
        }
    }
}