using Microsoft.EntityFrameworkCore;
using BlogProjectMVC2.Models;

namespace BlogProjectMVC2.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Users> Users { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Like> Likes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User → Post
            modelBuilder.Entity<Post>()
                .HasOne(p => p.User)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Category → Post
            modelBuilder.Entity<Post>()
                .HasOne(p => p.category)
                .WithMany(c => c.Posts)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Post → Comment
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.post)
                .WithMany(p => p.comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            // User → Comment
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany() // Eğer Users -> Comments koleksiyonun yoksa
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.NoAction); // veya Cascade de olabilir, sana bağlı

            // User → Like
            modelBuilder.Entity<Like>()
                .HasOne(l => l.User)
                .WithMany(u => u.Likes)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.NoAction); // çakışmayı önlemek için

            // Post → Like
            modelBuilder.Entity<Like>()
                .HasOne(l => l.Post)
                .WithMany(p => p.Likes)
                .HasForeignKey(l => l.PostId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
