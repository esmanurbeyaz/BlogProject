using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BlogProjectMVC2.Models
{
    public class Post
    {
        [Key]
        public int PostId { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string PostTitle { get; set; }

        [Required]
        public string PostContent { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime PublishDate { get; set; }
        public string? PostImageUrl { get; set; }
        //public string? PostSmallImageUrl { get; set; }
        public bool IsApproved { get; set; } = false; // admin onay

        [ForeignKey("CategoryId")]
        public int CategoryId { get; set; }
        public virtual Category? category { get; set; }

        [ForeignKey("UserId")]
        public virtual Users? User { get; set; }
        public int UserId { get; set; }

        public virtual ICollection<Like>? Likes { get; set; }
        public virtual ICollection<Comment>? comments { get; set; }

    }
}