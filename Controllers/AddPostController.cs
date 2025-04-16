using BlogProjectMVC2.Data;
using BlogProjectMVC2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;

namespace BlogProjectMVC2.Controllers
{
    [Authorize]
    public class AddPostController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public AddPostController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.Categories = _context.Categories
                                        .Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                                        {
                                            Value = c.CategoryId.ToString(),
                                            Text = c.CategoryName
                                        }).ToList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index([Bind("PostTitle,PostContent,CategoryId")] Post post, IFormFile imageFile)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    Debug.WriteLine("ModelState is not valid");
                    foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                    {
                        Debug.WriteLine($"Error: {error.ErrorMessage}");
                    }
                    ViewBag.Categories = _context.Categories
                                                .Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                                                {
                                                    Value = c.CategoryId.ToString(),
                                                    Text = c.CategoryName
                                                }).ToList();
                    return View(post);
                }

                
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                
                var newPost = new Post
                {
                    PostTitle = post.PostTitle,
                    PostContent = post.PostContent,
                    CategoryId = post.CategoryId,
                    UserId = userId,
                    PublishDate = DateTime.Now,
                    IsApproved = false,
                    PostImageUrl = null,
                    Likes = null,
                    comments = null
                };

                // Görsel 
                if (imageFile != null && imageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    newPost.PostImageUrl = "/uploads/" + uniqueFileName;
                }

                _context.Posts.Add(newPost);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Gönderiniz başarıyla paylaşıldı. Admin onayından sonra yayınlanacaktır.";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                ModelState.AddModelError("", "Gönderi paylaşılırken bir hata oluştu: " + ex.Message);
                ViewBag.Categories = _context.Categories
                                            .Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                                            {
                                                Value = c.CategoryId.ToString(),
                                                Text = c.CategoryName
                                            }).ToList();
                return View(post);
            }
        }
    }
}