using BlogProjectMVC2.Data;
using BlogProjectMVC2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;

namespace BlogProjectMVC2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Kategori
            var categories = await _context.Categories
                .Include(c => c.Posts)
                .ToListAsync();
            ViewBag.Categories = categories;

            // Post
            var posts = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.category)
                .Include(p => p.Likes)
                .Include(p => p.comments)
                .Where(p => p.IsApproved)
                .OrderByDescending(p => p.PublishDate)
                .ToListAsync();

            // Popüler
            var popularPosts = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.category)
                .Include(p => p.comments)
                .Where(p => p.IsApproved && p.comments != null)
                .OrderByDescending(p => p.comments.Count)
                .Take(5)
                .ToListAsync();
            ViewBag.PopularPosts = popularPosts;

            
            ViewBag.IsAuthenticated = User.Identity.IsAuthenticated;
            if (User.Identity.IsAuthenticated)
            {
                ViewBag.CurrentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            }

            return View(posts);
        }

        public async Task<IActionResult> CategoryPosts(int id)
        {
            
            var categories = await _context.Categories
                .Include(c => c.Posts)
                .ToListAsync();
            ViewBag.Categories = categories;

            
            var category = await _context.Categories
                .Include(c => c.Posts)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
            {
                return NotFound();
            }

            
            var posts = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.category)
                .Include(p => p.Likes)
                .Include(p => p.comments)
                .Where(p => p.CategoryId == id && p.IsApproved)
                .OrderByDescending(p => p.PublishDate)
                .ToListAsync();

            
            var popularPosts = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.category)
                .Include(p => p.comments)
                .Where(p => p.IsApproved && p.comments != null)
                .OrderByDescending(p => p.comments.Count)
                .Take(5)
                .ToListAsync();
            ViewBag.PopularPosts = popularPosts;

            ViewBag.IsAuthenticated = User.Identity.IsAuthenticated;
            if (User.Identity.IsAuthenticated)
            {
                ViewBag.CurrentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            }

            ViewBag.CategoryName = category.CategoryName;
            return View("Index", posts);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}