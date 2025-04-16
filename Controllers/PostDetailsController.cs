using Microsoft.AspNetCore.Mvc;
using BlogProjectMVC2.Models;
using Microsoft.EntityFrameworkCore;
using BlogProjectMVC2.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BlogProjectMVC2.Controllers
{
    public class PostDetailsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PostDetailsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int id)
        {
            var post = await _context.Posts
                .Include(p => p.category)
                .Include(p => p.User)
                .Include(p => p.comments)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(p => p.PostId == id);

            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddComment(int PostId, string CommentContent)
        {
            if (string.IsNullOrEmpty(CommentContent))
            {
                TempData["Error"] = "Yorum boş olamaz.";
                return RedirectToAction(nameof(Index), new { id = PostId });
            }

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int userId))
            {
                TempData["Error"] = "Kullanıcı bilgisi alınamadı.";
                return RedirectToAction(nameof(Index), new { id = PostId });
            }

            var comment = new Comment
            {
                PostId = PostId,
                UserId = userId,
                CommentContent = CommentContent,
                CommentPublishDate = DateTime.Now
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Yorumunuz başarıyla eklendi.";
            return RedirectToAction(nameof(Index), new { id = PostId });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var comment = await _context.Comments
                .Include(c => c.post)
                .FirstOrDefaultAsync(c => c.CommentId == commentId);

            if (comment == null)
            {
                return NotFound();
            }

            // Kullanıcı yorumun sahibi veya gönderinin sahibi mi kontrol et
            if (comment.UserId != userId && comment.post.UserId != userId)
            {
                return Forbid();
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Yorum başarıyla silindi.";
            return RedirectToAction(nameof(Index), new { id = comment.PostId });
        }
    }
}