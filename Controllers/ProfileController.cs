using BlogProjectMVC2.Data;
using BlogProjectMVC2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using BlogProjectMVC2.Enum;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace BlogProjectMVC2.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProfileController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _context.Users
                .Include(u => u.Posts)
                    .ThenInclude(p => p.Likes)
                .Include(u => u.Posts)
                    .ThenInclude(p => p.comments)
                .Include(u => u.Likes)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound();
            }

            var isAdmin = User.IsInRole("Admin");
            ViewBag.IsAdmin = isAdmin;

            if (isAdmin)
            {
                // Onay bekleyen gönderiler
                var pendingPosts = await _context.Posts
                    .Where(p => !p.IsApproved)
                    .Include(p => p.User)
                    .Include(p => p.category)
                    .Include(p => p.Likes)
                    .ToListAsync();
                ViewBag.PendingPosts = pendingPosts;

                // Onay bekleyen kullanıcılar
                var pendingUsers = await _context.Users
                    .Where(u => !u.IsApproved)
                    .ToListAsync();
                ViewBag.PendingUsers = pendingUsers;

                // Tüm kullanıcılar (admin paneli için)
                var allUsers = await _context.Users
                    .OrderBy(u => u.UserName)
                    .ToListAsync();
                ViewBag.AllUsers = allUsers;
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Users user)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", user);
            }

            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var existingUser = await _context.Users.FindAsync(userId);
                if (existingUser == null)
                {
                    return NotFound();
                }

                // Kullanıcı bilgilerini güncelle
                existingUser.Name = user.Name;
                existingUser.Surname = user.Surname;
                existingUser.UserName = user.UserName;
                existingUser.Email = user.Email;

                _context.Update(existingUser);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Profil bilgileriniz başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Profil güncellenirken bir hata oluştu: " + ex.Message);
                return View("Index", user);
            }
        }

        public async Task<IActionResult> MyPosts()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var posts = await _context.Posts
                .Include(p => p.category)
                .Include(p => p.comments)
                .Include(p => p.Likes)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.PublishDate)
                .ToListAsync();

            return View(posts);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApprovePost(int postId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
            {
                return NotFound();
            }

            post.IsApproved = true;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveUser(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            user.IsApproved = true;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeletePost(int postId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var post = await _context.Posts
                .FirstOrDefaultAsync(p => p.PostId == postId && p.UserId == userId);

            if (post == null)
            {
                return NotFound();
            }

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Gönderi başarıyla silindi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EditPost(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var post = await _context.Posts
                .Include(p => p.category)
                .FirstOrDefaultAsync(p => p.PostId == id && p.UserId == userId);

            if (post == null)
            {
                return NotFound();
            }

            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(post);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int id, [Bind("PostId,PostTitle,PostContent,CategoryId")] Post post)
        {
            if (id != post.PostId)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var existingPost = await _context.Posts
                .FirstOrDefaultAsync(p => p.PostId == id && p.UserId == userId);

            if (existingPost == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    existingPost.PostTitle = post.PostTitle;
                    existingPost.PostContent = post.PostContent;
                    existingPost.CategoryId = post.CategoryId;
                    existingPost.IsApproved = false; // Düzenlenen gönderi tekrar onay beklemeli

                    _context.Update(existingPost);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Gönderi başarıyla güncellendi.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.PostId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(post);
        }

        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.PostId == id);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleAdminRole(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            if (user.Role == RoleEnum.Admin)
            {
                user.Role = RoleEnum.Yazar;
            }
            else
            {
                user.Role = RoleEnum.Admin;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> LikePost(int postId, string returnUrl = null)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Kullanıcının daha önce bu gönderiyi beğenip beğenmediğini kontrol et
            var existingLike = await _context.Likes
                .FirstOrDefaultAsync(l => l.UserId == userId && l.PostId == postId);

            if (existingLike != null)
            {
                // Eğer zaten beğenilmişse, beğeniyi kaldır
                _context.Likes.Remove(existingLike);
            }
            else
            {
                // Beğeni ekle
                var like = new Like
                {
                    UserId = userId,
                    PostId = postId
                };
                _context.Likes.Add(like);
            }

            await _context.SaveChangesAsync();

            // Eğer returnUrl varsa oraya yönlendir, yoksa anasayfaya dön
            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UnlikePost(int postId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var like = await _context.Likes
                .FirstOrDefaultAsync(l => l.UserId == userId && l.PostId == postId);

            if (like != null)
            {
                _context.Likes.Remove(like);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}